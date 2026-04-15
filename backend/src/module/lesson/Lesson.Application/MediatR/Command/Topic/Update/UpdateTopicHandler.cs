namespace Lesson.Application.MediatR.Command.Topic.Update;

public record UpdateTopicCommand(
    Guid TopicId,
    string? Title,
    string? Description,
    int? EstimatedTimeMinutes,
    int? NewExamYear, 
    string? NewExamCode
) : IRequest<Result>;

public class UpdateTopicHandler(
        ITopicRepository topicRepository, 
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ILogger<UpdateTopicHandler> logger) 
    : IRequestHandler<UpdateTopicCommand, Result>
{
    private readonly ILogger<UpdateTopicHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(UpdateTopicCommand request, CancellationToken cancellationToken)
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
                    // Update fields if provided
                    if (request.Title is not null)
                        topicAggregate.UpdateTitle(request.Title);
                    if (request.Description is not null)
                        topicAggregate.UpdateDescription(request.Description);
                    if (request.EstimatedTimeMinutes is not null)
                        topicAggregate.UpdateEstimatedTime(request.EstimatedTimeMinutes.Value);
                    if (request.NewExamCode is not null&& request.NewExamYear > 0)
                        topicAggregate.UpdateExamInfo(request.NewExamYear.Value, request.NewExamCode ?? string.Empty  );

                    await _topicRepository.UpdateAsync(topicAggregate, cancellationToken);

                    _logger.LogInformation("[UpdateTopicHandler] Publishing {EventCount} domain events in-transaction", topicAggregate.DomainEvents.Count);
                    foreach (var domainEvent in topicAggregate.DomainEvents)
                    {
                        _logger.LogInformation("Publishing domain event {EventType} for topic {TopicId}", domainEvent.GetType().Name, topicAggregate.TopicId);
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }
                    topicAggregate.PopDomainEvents();
                }
                else
                    throw new KeyNotFoundException($"Không tìm thấy chủ đề với ID: {request.TopicId}");
            }, cancellationToken);

            return Result.SuccessResult("Cập nhật chủ đề thành công.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Topic not found with ID: {TopicId}", request.TopicId);
            return Result.FailureResult(
                "Không tìm thấy chủ đề với ID: " + request.TopicId,
                (int)ErrorCode.NOTFOUND
            );
        }
    }
}