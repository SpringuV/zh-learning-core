namespace Lesson.Application.MediatR.Command.Topic;

public record ReOrderTopicCommand(Guid CourseId, List<Guid> OrderedTopicIds) : IRequest<Result>;

public class ReOrderTopicHandler(
        ITopicRepository topicRepository, 
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ILogger<ReOrderTopicHandler> logger)
    : IRequestHandler<ReOrderTopicCommand, Result>
{
    private readonly ILogger<ReOrderTopicHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task<Result> Handle(ReOrderTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.OrderedTopicIds.Count == 0)
                throw new ArgumentException("Danh sách chủ đề sắp xếp không được để trống.");

            if (request.OrderedTopicIds.Count != request.OrderedTopicIds.Distinct().Count())
                throw new ArgumentException("Danh sách chủ đề sắp xếp không được chứa ID trùng lặp.");

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                // Reorder bằng 1 SQL command (2-phase trong CTE) để tránh va chạm unique index (CourseId, OrderIndex).
                await _topicRepository.ReorderByIdsAndCourseIdAsync(
                    request.CourseId,
                    request.OrderedTopicIds,
                    cancellationToken);
                
                var reorderEvent = new TopicReOrderedEvent(
                    CourseId: request.CourseId,
                    OrderedTopicIds: request.OrderedTopicIds,
                    UpdatedAt: DateTime.UtcNow
                );
                
                await _publisher.Publish(reorderEvent, cancellationToken);
            }, cancellationToken);
            
            return Result.SuccessResult(message: "Reorder successfully.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid topic reorder request. Details: {Message}", ex.Message);
            return Result.FailureResult("Dữ liệu sắp xếp không hợp lệ. " + ex.Message, (int)ErrorCode.INVALID_ARGUMENT);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found during reorder operation. Details: {Message}", ex.Message);
            return Result.FailureResult("Không tìm thấy khóa học trong quá trình sắp xếp. " + ex.Message, (int)ErrorCode.NOTFOUND);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during topic reorder operation. Details: {Message}", ex.Message);
            return Result.FailureResult("Đã xảy ra lỗi không mong muốn trong quá trình sắp xếp chủ đề.", (int)
            ErrorCode.INTERNAL_ERROR);
        }
    }
}