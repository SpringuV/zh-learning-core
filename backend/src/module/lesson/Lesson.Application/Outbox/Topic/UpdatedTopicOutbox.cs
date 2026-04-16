namespace Lesson.Application.Outbox.Topic;

#region Publish
public sealed class PublishTopicOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicPublishedEvent>
{
    public Task Handle(TopicPublishedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicPublishedIntegrationEvent(
                notification.TopicId,
                notification.PublishedAt
            ),
            cancellationToken);
}
#endregion
#region UnPublish
public sealed class UnPublishTopicOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicUnpublishedEvent>
{
    public Task Handle(TopicUnpublishedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicUnpublishedIntegrationEvent(
                notification.TopicId,
                notification.UnpublishedAt
            ),
            cancellationToken);
}
#endregion
#region ReOrder
public sealed class TopicReOrderedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicReOrderedEvent>
{
    public Task Handle(TopicReOrderedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicReOrderedIntegrationEvent(
                CourseId: notification.CourseId,
                OrderedTopicIds: notification.OrderedTopicIds,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region Title
public sealed class TopicTitleUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicTitleUpdatedEvent>
{
    public Task Handle(TopicTitleUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicTitleUpdatedIntegrationEvent(
                TopicId: notification.TopicId,
                CourseId: notification.CourseId,
                NewTitle: notification.NewTitle,
                NewSlug: notification.NewSlug,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion

#region Description
public sealed class TopicDescriptionUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicDescriptionUpdatedEvent>
{
    public Task Handle(TopicDescriptionUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicDescriptionUpdatedIntegrationEvent(
                TopicId: notification.TopicId,
                CourseId: notification.CourseId,
                NewDescription: notification.NewDescription,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region EstimatedTime
public sealed class TopicEstimatedTimeUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicEstimatedTimeUpdatedEvent>
{
    public Task Handle(TopicEstimatedTimeUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicEstimatedTimeUpdatedIntegrationEvent(
                TopicId: notification.TopicId,
                CourseId: notification.CourseId,
                NewEstimatedTimeMinutes: notification.NewEstimatedTimeMinutes,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ExamInfo
public sealed class TopicExamInfoUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicExamInfoUpdatedEvent>
{
    public Task Handle(TopicExamInfoUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicExamInfoUpdatedIntegrationEvent(
                TopicId: notification.TopicId,
                CourseId: notification.CourseId,
                NewExamYear: notification.NewExamYear,
                NewExamCode: notification.NewExamCode,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region TotalExercises
public sealed class TopicTotalExercisesUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicTotalExercisesUpdatedEvent>
{
    public Task Handle(TopicTotalExercisesUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicTotalExercisesUpdatedIntegrationEvent(
                notification.TopicId,
                notification.TotalExercises,
                notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region Delete
public sealed class DeletedTopicOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicDeletedEvent>
{
    public Task Handle(TopicDeletedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new TopicDeletedIntegrationEvent(
                notification.TopicId
            ),
            cancellationToken);
}
#endregion