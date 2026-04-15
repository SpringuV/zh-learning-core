using HanziAnhVu.Shared.Domain;

namespace Lesson.Application.MediatR.Command.Exercise.Create;

public sealed record CreateExerciseCommand(
    Guid TopicId,
    string Description,
    ExerciseType ExerciseType,
    ExerciseDifficulty Difficulty,
    SkillType SkillType,
    ExerciseContext ExerciseContext,
    string Question,
    List<ExerciseOption>? Options,
    string CorrectAnswer,
    string Explanation,
    string? AudioUrl = null,
    string? ImageUrl = null
) : IRequest<Result<CreateExerciseResponseDTO>>;

public class CreateExerciseCommandHandler(
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        IExerciseRepository exerciseRepository,
        ITopicRepository topicRepository) 
    : IRequestHandler<CreateExerciseCommand, Result<CreateExerciseResponseDTO>>
{
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));

    public async Task<Result<CreateExerciseResponseDTO>> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // phase 1: validate input (đã có FluentValidation đảm nhiệm, nhưng nếu cần validate thêm phức tạp hơn thì có thể làm ở đây)
            ExerciseAggregate exerciseAggregate = null!;
            var topic = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
            if (topic is null)
            {
                return Result<CreateExerciseResponseDTO>.FailureResult(
                    $"Topic with id '{request.TopicId}' was not found.",
                    (int)ErrorCode.NOTFOUND
                );
            }

            // Create aggregate + publish domain events (write outbox) + save DB in one transaction
            var maxOrder = await _exerciseRepository.GetMaxOrderIndexByTopicIdAsync(request.TopicId, cancellationToken);
            // phase 2: create aggregate, publish domain events, save to DB trong cùng một transaction để đảm bảo consistency. Nếu có lỗi ở bất kỳ bước nào thì toàn bộ transaction sẽ rollback và không có dữ liệu bị ghi vào database.
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                exerciseAggregate = ExerciseAggregate.CreateExercise(
                    topicId: request.TopicId,
                    description: request.Description,
                    exerciseType: request.ExerciseType,
                    difficulty: request.Difficulty,
                    skillType: request.SkillType,
                    context: request.ExerciseContext,
                    question: request.Question,
                    options: request.Options,
                    correctAnswer: request.CorrectAnswer,
                    explanation: request.Explanation,
                    orderIndex: maxOrder.HasValue ? maxOrder.Value + 1 : 1, // nếu đã có bài tập nào trong topic thì orderIndex = maxOrder + 1, nếu chưa có bài tập nào thì orderIndex = 1
                    audioUrl: request.AudioUrl ?? "",
                    imageUrl: request.ImageUrl ?? ""
                );

                await _exerciseRepository.AddAsync(exerciseAggregate, cancellationToken);
                // update totalExercises của topic
                topic.IncrementTotalExercises();
                await _topicRepository.UpdateAsync(topic, cancellationToken);

                foreach (var domainEvent in exerciseAggregate.DomainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                exerciseAggregate.PopDomainEvents();

                foreach (var domainEvent in topic.DomainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                topic.PopDomainEvents();
            }, cancellationToken);
            // phase 3: return result
            return Result<CreateExerciseResponseDTO>.SuccessResult(
                new CreateExerciseResponseDTO(exerciseAggregate.ExerciseId),
                "Bài tập đã được tạo thành công."
            );
        } 
        catch (ArgumentException ex)
        {
            return Result<CreateExerciseResponseDTO>.FailureResult(
                $"Invalid input: {ex.Message}",
                (int)ErrorCode.INVALID_ARGUMENT
            );
        }
        catch (Exception ex)
        {
            // Log exception details here if needed
            return Result<CreateExerciseResponseDTO>.FailureResult(
                $"An error occurred while creating the exercise. Please try again later. {ex.Message}",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}