namespace Lesson.Application.Outbox.Topic;

public sealed class TopicCreatedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<TopicCreatedEvent>
{
    public Task Handle(TopicCreatedEvent notification, CancellationToken cancellationToken)
        => lessonOutboxWriter.EnqueueAsync(
            new TopicCreatedIntegrationEvent(
                notification.TopicId,
                notification.CourseId,
                notification.Title,
                notification.Description,
                notification.Slug,
                notification.TopicType.ToString(),
                notification.EstimatedTimeMinutes,
                notification.ExamYear ?? 0,
                notification.ExamCode,
                notification.OrderIndex,
                notification.IsPublished,
                notification.TotalExercises,
                notification.CreatedAt,
                notification.UpdatedAt,
                notification.TotalExercisesPublished
            ),
            cancellationToken);
}