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
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                var topics = await _topicRepository.GetByIdsAndCourseIdAsync(request.CourseId, request.OrderedTopicIds, cancellationToken);
                
                if (topics.Count() != request.OrderedTopicIds.Count)
                    throw new KeyNotFoundException("Một số chủ đề không tồn tại trong khóa học này");
                
                for (int i = 0; i < topics.Count(); i++)
                {
                    topics.ElementAt(i).UpdateOrderIndex(i + 1);
                }

                await _topicRepository.UpdateRangeAsync(topics, cancellationToken);
                
                var reorderEvent = new TopicReOrderedEvent(
                    CourseId: request.CourseId,
                    OrderedTopicIds: request.OrderedTopicIds,
                    UpdatedAt: DateTime.UtcNow
                );
                
                await _publisher.Publish(reorderEvent, cancellationToken);
            }, cancellationToken);
            
            return Result.SuccessResult(message: "Reorder successfully.");
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