namespace Lesson.Application.MediatR.Command.Topic;

public record PublishTopicCommand(Guid TopicId) : IRequest<Result>;

public class PublishTopicHandler(ITopicRepository topicRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<PublishTopicHandler> logger) : IRequestHandler<PublishTopicCommand, Result>
{
    private readonly ILogger<PublishTopicHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(PublishTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            TopicAggregate? topicAggregate = null;
            
            if (request.TopicId == Guid.Empty)
                return Result.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                topicAggregate = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
                if (topicAggregate is not null)
                {
                    topicAggregate.Publish();
                    await _topicRepository.UpdateAsync(topicAggregate, cancellationToken);

                    _logger.LogInformation("[PublishTopicHandler] Publishing {EventCount} domain events in-transaction", topicAggregate.DomainEvents.Count);
                    foreach (var domainEvent in topicAggregate.DomainEvents)
                    {
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }
                    topicAggregate.PopDomainEvents();
                }
                else
                    throw new KeyNotFoundException($"Không tìm thấy chủ đề với ID: {request.TopicId}");
            }, cancellationToken);

            return Result.SuccessResult(
                "Chủ đề đã được xuất bản thành công."
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
                "Chủ đề với ID " + request.TopicId + " đã được xuất bản.",
                (int)ErrorCode.ALREADY_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during topic publish operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                "Đã xảy ra lỗi không mong muốn trong quá trình xuất bản chủ đề.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}