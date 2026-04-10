using Lesson.Contracts;
using Lesson.Domain.Entities.Events;

namespace Lesson.Application.Outbox.Course;

public sealed class CourseCreatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<CourseCreatedEvent>
{
    public Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new CourseCreatedIntegrationEvent(
                notification.CourseId,
                notification.Title, 
                notification.Description, 
                notification.OrderIndex, 
                notification.Slug,
                notification.CreatedAt,
                notification.UpdatedAt
            ),
            cancellationToken);
}