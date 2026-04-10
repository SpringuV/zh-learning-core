using Lesson.Contracts;
using Lesson.Domain.Entities.Events;

namespace Lesson.Application.Outbox.Course;

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
