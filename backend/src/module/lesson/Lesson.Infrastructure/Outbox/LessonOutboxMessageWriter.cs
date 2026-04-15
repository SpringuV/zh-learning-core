namespace Lesson.Infrastructure.Outbox;

public class LessonOutboxMessageWriter(
    LessonDbContext dbContext,
    ILogger<LessonOutboxMessageWriter> logger) : ILessonOutboxWriter
{
    private readonly LessonDbContext _dbContext = dbContext;
    private readonly ILogger<LessonOutboxMessageWriter> _logger = logger;

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessageSerialization.Create<LessonModuleOutboxMessage>(integrationEvent);
        // Add the message to the DbContext. It will be saved to the database when the transaction is committed.
        _dbContext.LessonOutboxMessages.Add(message);
        _logger.LogInformation("[LessonOutboxMessageWriter] Enqueued integration event {EventType} with id {MessageId}", integrationEvent.GetType().Name, message.Id);
        return Task.CompletedTask;
    }
}