namespace Lesson.Application.Outbox.Course;

#region Title
public sealed class UpdatedCourseTitleOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseTitleUpdatedEvent>
{
    public Task Handle(CourseTitleUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseTitleUpdatedIntegrationEvent(
                notification.CourseId,
                notification.NewTitle,
                notification.NewSlug,
                notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region Description
public sealed class UpdatedCourseDescriptionOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseDescriptionUpdatedEvent>
{
    public Task Handle(CourseDescriptionUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseDescriptionUpdatedIntegrationEvent(
                notification.CourseId,
                notification.NewDescription,
                notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion

#region HskLevel
public sealed class UpdatedCourseHskLevelOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseHskLevelUpdatedEvent>
{
    public Task Handle(CourseHskLevelUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseHskLevelUpdatedIntegrationEvent(
                notification.CourseId,
                notification.NewHskLevel,
                notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion

#region ReOrder
public sealed class CourseReOrderedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseReOrderedEvent>
{
    public Task Handle(CourseReOrderedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseReOrderedIntegrationEvent(
                OrderedCourseIds: notification.OrderedCourseIds,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region Publish
public sealed class PublishCourseOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CoursePublishedDomainEvent>
{
    public Task Handle(CoursePublishedDomainEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CoursePublishedIntegrationEvent(
                notification.CourseId,
                notification.PublishedAt
            ),
            cancellationToken);
}
#endregion
#region UnPublish
public sealed class UnPublishCourseOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseUnpublishedDomainEvent>
{
    public Task Handle(CourseUnpublishedDomainEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseUnpublishedIntegrationEvent(
                notification.CourseId,
                notification.UnpublishedAt
            ),
            cancellationToken);
}
#endregion

#region TotalTopics
public sealed class CourseTotalTopicsUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseTotalTopicsUpdatedEvent>
{
    public Task Handle(CourseTotalTopicsUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseTotalTopicsUpdatedIntegrationEvent(
                notification.CourseId,
                notification.TotalTopics,
                notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion

#region Delete
public sealed class DeletedCourseOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseDeletedEvent>
{
    public Task Handle(CourseDeletedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseDeletedIntegrationEvent(
                notification.CourseId
            ),
            cancellationToken);
}
#endregion

#region TotalTopicsPublished
public sealed class CourseTotalTopicsPublishedUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseTotalTopicsPublishedUpdatedEvent>
{
    public Task Handle(CourseTotalTopicsPublishedUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseTotalTopicsPublishedUpdatedIntegrationEvent(
                notification.CourseId,
                notification.TotalTopicsPublished,
                notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion