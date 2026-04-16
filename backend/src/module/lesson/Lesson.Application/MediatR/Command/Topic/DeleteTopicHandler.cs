namespace Lesson.Application.MediatR.Command.Topic;

public record DeleteTopicCommand(Guid TopicId) : IRequest<Result>;

public class DeleteTopicCommandHandler(
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ITopicRepository topicRepository,
        ILogger<DeleteTopicCommandHandler> logger) 
    : IRequestHandler<DeleteTopicCommand, Result>
{
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILogger<DeleteTopicCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> Handle(DeleteTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check if topic exists and can be deleted, if not, throw exception, the exception will be caught and return appropriate failure result
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                var topicAggregate = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken) ?? throw new KeyNotFoundException($"Topic with id '{request.TopicId}' was not found.");
                topicAggregate.Delete();
                // significant domain logic is handled in the aggregate, including validation and raising domain events, 
                // so we just need to call the delete method on the aggregate and then save changes,
                // the domain events will be automatically published after saving changes, 
                // we don't need to manually publish the TopicDeletedEvent here
                await _topicRepository.DeleteAsync(topicAggregate.TopicId, cancellationToken);

                // Publish event
                foreach (var domainEvent in topicAggregate.DomainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
            }, cancellationToken);

            return Result.SuccessResult("Chủ đề đã được xóa thành công.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Topic not found with ID: {TopicId}", request.TopicId);
            return Result.FailureResult(
                "Không tìm thấy chủ đề.",
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Topic with ID {TopicId} cannot be deleted.", request.TopicId);
            return Result.FailureResult(
                ex.Message,
                (int)ErrorCode.ALREADY_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during topic delete operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                ex.Message,
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}