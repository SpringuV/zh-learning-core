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
            if (request.OrderedExerciseIds.Count == 0)
                throw new ArgumentException("Danh sách bài tập sắp xếp không được để trống.");

            if (request.OrderedExerciseIds.Count != request.OrderedExerciseIds.Distinct().Count())
                throw new ArgumentException("Danh sách bài tập sắp xếp không được chứa ID trùng lặp.");

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                // Reorder bằng 1 SQL command (2-phase trong CTE) để đảm bảo thứ tự ổn định và tránh va chạm index trong tương lai.
                await _exerciseRepository.ReorderByIdsAndTopicIdAsync(
                    request.TopicId,
                    request.OrderedExerciseIds,
                    cancellationToken);
                
                var reorderEvent = new ExerciseReOrderedEvent(
                    TopicId: request.TopicId,
                    OrderedExerciseIds: request.OrderedExerciseIds,
                    UpdatedAt: DateTime.UtcNow
                );
                
                await _publisher.Publish(reorderEvent, cancellationToken);
            }, cancellationToken);
            
            return Result.SuccessResult(message: "Reorder successfully.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid exercise reorder request. Details: {Message}", ex.Message);
            return Result.FailureResult("Dữ liệu sắp xếp không hợp lệ. " + ex.Message, (int)ErrorCode.INVALID_ARGUMENT);
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