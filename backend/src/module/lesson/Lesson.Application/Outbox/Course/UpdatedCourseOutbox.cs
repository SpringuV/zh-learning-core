using Lesson.Contracts;
using Lesson.Domain.Entities.Events;

namespace Lesson.Application.Outbox.Course;

public sealed class UpdatedCourseOutbox(ILessonOutboxWriter outboxWriter)
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

public sealed class UpdatedCourseOrderIndexOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseOrderIndexUpdatedEvent>
{
    public Task Handle(CourseOrderIndexUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseOrderIndexUpdatedIntegrationEvent(
                notification.CourseId,
                notification.NewOrderIndex,
                notification.UpdatedAt
            ),
            cancellationToken);
}

