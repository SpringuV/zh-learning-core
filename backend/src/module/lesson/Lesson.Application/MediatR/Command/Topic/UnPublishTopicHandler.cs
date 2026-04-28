namespace Lesson.Application.MediatR.Command.Topic;

public record UnPublishTopicCommand(Guid TopicId) : IRequest<Result>;

public class UnPublishTopicHandler(ICourseRepository courseRepository, ITopicRepository topicRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<UnPublishTopicHandler> logger) : IRequestHandler<UnPublishTopicCommand, Result>
{
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILogger<UnPublishTopicHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(UnPublishTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            TopicAggregate? topicAggregate = null;
            CourseAggregate? courseAggregate = null;
            if (request.TopicId == Guid.Empty)
                return Result.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                topicAggregate = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
                if (topicAggregate is not null)
                {
                    courseAggregate = await _courseRepository.GetByIdAsync(topicAggregate.CourseId, cancellationToken);
                    if (courseAggregate is null)                    {
                        _logger.LogWarning("Course with ID {CourseId} not found when unpublishing topic. TopicId: {TopicId}", topicAggregate.CourseId, request.TopicId);
                        throw new KeyNotFoundException($"Không tìm thấy khóa học với ID: {topicAggregate.CourseId} cho chủ đề với ID: {request.TopicId}");
                    }
                    courseAggregate.DecreaseTotalTopicsPublished();
                    await _courseRepository.UpdateAsync(courseAggregate, cancellationToken);
                    topicAggregate.UnPublish();
                    await _topicRepository.UpdateAsync(topicAggregate, cancellationToken);

                    _logger.LogInformation("[UnPublishTopicHandler] Publishing {EventCount} domain events in-transaction", topicAggregate.DomainEvents.Count);
                    foreach (var domainEvent in topicAggregate.DomainEvents)
                    {
                        _logger.LogInformation("Publishing domain event {EventType} for topic {TopicId}", domainEvent.GetType().Name, topicAggregate.TopicId);
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }
                    topicAggregate.PopDomainEvents();
                    foreach (var domainEvent in courseAggregate.DomainEvents)
                    {
                        _logger.LogInformation("Publishing domain event {EventType} for course {CourseId}", domainEvent.GetType().Name, courseAggregate.CourseId);
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }
                    courseAggregate.PopDomainEvents();
                }
                else
                    throw new KeyNotFoundException($"Không tìm thấy chủ đề với ID: {request.TopicId}");
            }, cancellationToken);

            return Result.SuccessResult(
                "Chủ đề đã được hủy xuất bản thành công."
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Topic not found with ID: {TopicId}", request.TopicId);
            return Result.FailureResult(
                "Không tìm thấy chủ đề với ID: " + request.TopicId,
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Topic with ID {TopicId} is already published.", request.TopicId);
            return Result.FailureResult(
                "Chủ đề chưa được xuất bản.",
                (int)ErrorCode.NOT_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during topic publish operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                "Đã xảy ra lỗi không mong muốn trong quá trình hủy xuất bản chủ đề.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}