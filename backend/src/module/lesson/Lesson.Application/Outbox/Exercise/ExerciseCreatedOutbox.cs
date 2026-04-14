using Lesson.Domain.Entities.Events;

namespace Lesson.Application.Outbox.Exercise;

public sealed class ExerciseCreatedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<ExerciseCreatedEvent>
{
    public Task Handle(ExerciseCreatedEvent notification, CancellationToken cancellationToken)
        => lessonOutboxWriter.EnqueueAsync(
            new ExerciseCreatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                TopicId: notification.TopicId,
                Description: notification.Description,
                Question: notification.Question,
                CorrectAnswer: notification.CorrectAnswer,
                ExerciseType: notification.ExerciseType,
                SkillType: notification.SkillType,
                Difficulty: notification.Difficulty,
                Context: notification.Context,
                AudioUrl: notification.AudioUrl,
                ImageUrl: notification.ImageUrl,
                Slug: notification.Slug,
                Explanation: notification.Explanation,
                OrderIndex: notification.OrderIndex,
                IsPublished: notification.IsPublished,
                CreatedAt: notification.CreatedAt,
                UpdatedAt: notification.UpdatedAt,
                Options: notification.Options ?? []
            ),
            cancellationToken);
}