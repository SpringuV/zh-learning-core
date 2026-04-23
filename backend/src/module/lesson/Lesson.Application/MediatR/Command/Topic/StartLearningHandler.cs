namespace Lesson.Application.MediatR.Command.Topic;

public record StartLearningCommand(Guid UserId, Guid TopicId) : IRequest<Result<StartLearningResponseDTO>>;

// validate input, load topic + published exercises, create UserTopicExerciseSessionAggregate + TopicProgressAggregate (if not exist) trong 1 transaction, publish domain events, return success.
public class ValidatorStartLearning : AbstractValidator<StartLearningCommand>
{
    public ValidatorStartLearning()
    {
        RuleFor(cmd => cmd.UserId).NotEmpty().WithMessage("UserId không được để trống.");
        RuleFor(cmd => cmd.TopicId).NotEmpty().WithMessage("TopicId không được để trống.");
    }
}

public class StartLearningHandler(ITopicProgressRepository topicProgressRepository,
    IPublisher publisher,
    ILessonUnitOfWork unitOfWork,
    ITopicRepository topicRepository,
    IExerciseRepository exerciseRepository,
    IUserTopicExerciseSessionRepository userTopicExerciseSessionRepository,
    ILogger<StartLearningHandler> logger) : IRequestHandler<StartLearningCommand, Result<StartLearningResponseDTO>>
{
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILogger<StartLearningHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUserTopicExerciseSessionRepository _userTopicExerciseSessionRepository = userTopicExerciseSessionRepository ?? throw new ArgumentNullException(nameof(userTopicExerciseSessionRepository));
    private readonly ITopicProgressRepository _topicProgressRepository = topicProgressRepository ?? throw new ArgumentNullException(nameof(topicProgressRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    public async Task<Result<StartLearningResponseDTO>> Handle(StartLearningCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var topicAggregate = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
            if (topicAggregate == null)
            {
                _logger.LogWarning("Topic with ID {TopicId} not found when starting learning. UserId: {UserId}", request.TopicId, request.UserId);
                return Result<StartLearningResponseDTO>.FailureResult("Không tìm thấy chủ đề học.", (int)ErrorCode.NOTFOUND);
            }

            // load tất cả bài tập đã xuất bản của chủ đề, sắp xếp theo OrderIndex và ExerciseId để đảm bảo thứ tự ổn định.
            var publishedExercises = (await _exerciseRepository.GetByTopicIdAsync(request.TopicId, cancellationToken))
                .Where(exercise => exercise.IsPublished)
                .OrderBy(exercise => exercise.OrderIndex)
                .ThenBy(exercise => exercise.ExerciseId) // đảm bảo thứ tự ổn định khi OrderIndex trùng nhau
                .ToList();

            if (publishedExercises.Count == 0)
            {
                _logger.LogWarning("Topic {TopicId} has no published exercises when starting learning. UserId: {UserId}", request.TopicId, request.UserId);
                return Result<StartLearningResponseDTO>.FailureResult("Chủ đề chưa có bài tập đã xuất bản.", (int)ErrorCode.NOTFOUND);
            }

            UserTopicExerciseSessionAggregate userTopicExerciseSessionAggregate = null!;
            TopicProgressAggregate topicProgressAggregate = null!;

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                userTopicExerciseSessionAggregate = UserTopicExerciseSessionAggregate.Create(request.UserId, request.TopicId);
                var sessionItems = publishedExercises.Select((exercise, index) =>
                    UserTopicExerciseSessionItem.Create(
                        userTopicExerciseSessionAggregate.SessionId,
                        exercise.ExerciseId,
                        index + 1,
                        exercise.OrderIndex)).ToList();
                userTopicExerciseSessionAggregate.SetSessionItems(sessionItems);

                // Kiểm tra nếu đã có topic progress thì không tạo mới, tránh mất lịch sử cũ. Nếu chưa có thì tạo mới.
                var existingTopicProgress = await _topicProgressRepository.GetByUserIdAndTopicIdAsync(request.UserId, request.TopicId, cancellationToken);
                topicProgressAggregate = existingTopicProgress ?? TopicProgressAggregate.Create(request.UserId, request.TopicId);

                await _userTopicExerciseSessionRepository.AddAsync(userTopicExerciseSessionAggregate, cancellationToken);

                // Nếu chưa có progress thì mới thêm mới, nếu đã có rồi thì chỉ update (vì có thể có domain event mới từ việc tạo session).
                if (existingTopicProgress is null)
                {
                    await _topicProgressRepository.AddAsync(topicProgressAggregate, cancellationToken);
                }

                foreach (var domainEvent in topicProgressAggregate.DomainEvents)
                {
                    _logger.LogInformation("Publishing {EventType} for topic progress. UserId: {UserId}, TopicId: {TopicId}", domainEvent.GetType().Name, request.UserId, request.TopicId);
                    await _publisher.Publish(domainEvent, cancellationToken);
                }

                foreach (var domainEvent in userTopicExerciseSessionAggregate.DomainEvents)
                {
                    _logger.LogInformation("Publishing {EventType} for new exercise session. UserId: {UserId}, TopicId: {TopicId}", domainEvent.GetType().Name, request.UserId, request.TopicId);
                    await _publisher.Publish(domainEvent, cancellationToken);
                }

                topicProgressAggregate.PopDomainEvents();
                userTopicExerciseSessionAggregate.PopDomainEvents();
            }, cancellationToken);

            var response = BuildStartLearningResponse(userTopicExerciseSessionAggregate, topicProgressAggregate, publishedExercises);
            return Result<StartLearningResponseDTO>.SuccessResult(response, message: "Bắt đầu học chủ đề thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when starting learning for TopicId {TopicId} and UserId {UserId}. Details: {Message}", request.TopicId, request.UserId, ex.Message);
            return Result<StartLearningResponseDTO>.FailureResult("Đã xảy ra lỗi không mong muốn khi bắt đầu học. Vui lòng thử lại sau.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }

    private static StartLearningResponseDTO BuildStartLearningResponse(
        UserTopicExerciseSessionAggregate sessionAggregate,
        TopicProgressAggregate topicProgressAggregate,
        IReadOnlyList<ExerciseAggregate> publishedExercises)
    {
        ArgumentNullException.ThrowIfNull(sessionAggregate);
        ArgumentNullException.ThrowIfNull(topicProgressAggregate);
        ArgumentNullException.ThrowIfNull(publishedExercises);

        var sessionItems = sessionAggregate.SessionItems
            .Select(item => new StartLearningSessionItemDTO(
                SessionItemId: item.SessionItemId,
                ExerciseId: item.ExerciseId,
                SequenceNo: item.SequenceNo,
                OrderIndex: item.OrderIndex,
                AttemptId: item.AttemptId,
                Status: item.Status.ToString(),
                ViewedAt: item.ViewedAt,
                AnsweredAt: item.AnsweredAt))
            .ToList();

        var firstExercise = publishedExercises[0];
        var firstExerciseDto = new StartLearningExerciseDTO(
            ExerciseId: firstExercise.ExerciseId,
            TopicId: firstExercise.TopicId,
            OrderIndex: firstExercise.OrderIndex,
            Description: firstExercise.Description,
            Question: firstExercise.Question,
            ExerciseType: firstExercise.ExerciseType.ToString(),
            SkillType: firstExercise.SkillType.ToString(),
            Difficulty: firstExercise.Difficulty.ToString(),
            Context: firstExercise.Context.ToString(),
            AudioUrl: firstExercise.AudioUrl,
            ImageUrl: firstExercise.ImageUrl,
            Options: [.. firstExercise.Options
                .Select(option => new StartLearningExerciseOptionDTO(
                    Id: option.Id,
                    Text: option.Text))]);

        return new StartLearningResponseDTO(
            SessionId: sessionAggregate.SessionId,
            TopicProgressId: topicProgressAggregate.TopicProgressId,
            TotalExercises: sessionAggregate.TotalExercises,
            CurrentSequenceNo: sessionAggregate.CurrentSequenceNo,
            SessionItems: sessionItems,
            FirstExercise: firstExerciseDto
        );
    }
}