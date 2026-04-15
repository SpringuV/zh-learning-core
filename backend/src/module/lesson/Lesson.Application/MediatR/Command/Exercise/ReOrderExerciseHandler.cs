namespace Lesson.Application.MediatR.Command.Exercise;

public record ReOrderExerciseCommand(Guid TopicId, List<Guid> OrderedExerciseIds) : IRequest<Result>;

public class ReOrderExerciseHandler(
        IExerciseRepository exerciseRepository, 
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ILogger<ReOrderExerciseHandler> logger)
    : IRequestHandler<ReOrderExerciseCommand, Result>
{
    private readonly ILogger<ReOrderExerciseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task<Result> Handle(ReOrderExerciseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                var exercises = await _exerciseRepository.GetByTopicIdAndIdsAsync(request.TopicId, request.OrderedExerciseIds, cancellationToken);
                
                if (exercises.Count() != request.OrderedExerciseIds.Count)
                    throw new KeyNotFoundException("Một số bài tập không tồn tại trong chủ đề này");
                
                for (int i = 0; i < exercises.Count(); i++)
                {
                    exercises.ElementAt(i).UpdateOrderIndex(i + 1);
                }

                await _exerciseRepository.UpdateRangeAsync(exercises, cancellationToken);
                
                var reorderEvent = new ExerciseReOrderedEvent(
                    TopicId: request.TopicId,
                    OrderedExerciseIds: request.OrderedExerciseIds,
                    UpdatedAt: DateTime.UtcNow
                );
                
                await _publisher.Publish(reorderEvent, cancellationToken);
            }, cancellationToken);
            
            return Result.SuccessResult(message: "Reorder successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Topic not found during reorder operation. Details: {Message}", ex.Message);
            return Result.FailureResult("Không tìm thấy chủ đề trong quá trình sắp xếp. " + ex.Message, (int)ErrorCode.NOTFOUND);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during exercise reorder operation. Details: {Message}", ex.Message);
            return Result.FailureResult("Đã xảy ra lỗi không mong muốn trong quá trình sắp xếp bài tập.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}