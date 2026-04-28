namespace Lesson.Application.Outbox.Exercise;

#region Publish
public sealed class PublishExerciseOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExercisePublishedEvent>
{
    public Task Handle(ExercisePublishedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExercisePublishedIntegrationEvent(
                notification.ExerciseId,
                notification.PublishedAt
            ),
            cancellationToken);
}
#endregion
#region UnPublish
public sealed class UnPublishExerciseOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseUnpublishedEvent>
{
    public Task Handle(ExerciseUnpublishedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseUnpublishedIntegrationEvent(
                notification.ExerciseId,
                notification.UnpublishedAt
            ),
            cancellationToken);
}
#endregion

#region ReOrder
public sealed class ExerciseReOrderedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseReOrderedEvent>
{
    public Task Handle(ExerciseReOrderedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseReOrderedIntegrationEvent(
                TopicId: notification.TopicId,
                OrderedExerciseIds: notification.OrderedExerciseIds,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion

#region ExerciseType
public sealed class ExerciseTypeUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseTypeUpdatedEvent>
{
    public Task Handle(ExerciseTypeUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseTypeUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                TopicId: notification.TopicId,
                NewExerciseType: notification.NewExerciseType,
                NewSlug: notification.NewSlug,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion

#region ExerciseSkillType
public sealed class ExerciseSkillTypeUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseSkillTypeUpdatedEvent>
{
    public Task Handle(ExerciseSkillTypeUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseSkillTypeUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                TopicId: notification.TopicId,
                NewSkillType: notification.NewSkillType,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ExerciseContext
public sealed class ExerciseContextUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseContextUpdatedEvent>
{
    public Task Handle(ExerciseContextUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseContextUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                TopicId: notification.TopicId,
                NewContext: notification.NewContext,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ExerciseDescription
public sealed class ExerciseDescriptionUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseDescriptionUpdatedEvent>
{
    public Task Handle(ExerciseDescriptionUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseDescriptionUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewDescription: notification.NewDescription,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ExerciseOptions

public sealed class ExerciseOptionsUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseOptionsUpdatedEvent>
{
    public Task Handle(ExerciseOptionsUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseOptionsUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewOptions: notification.NewOptions,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ExerciseQuestion
public sealed class ExerciseQuestionUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseQuestionUpdatedEvent>
{
    public Task Handle(ExerciseQuestionUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseQuestionUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewQuestion: notification.NewQuestion,
                NewSlug: notification.NewSlug,
                UpdatedAt: notification.UpdatedAt),
            cancellationToken);
}
#endregion
#region  CorrectAnswer
public sealed class ExerciseCorrectAnswerUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseCorrectAnswerUpdatedEvent>
{
    public Task Handle(ExerciseCorrectAnswerUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseCorrectAnswerUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewCorrectAnswer: notification.NewCorrectAnswer,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ExerciseDifficulty
public sealed class ExerciseDifficultyUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseDifficultyUpdatedEvent>
{
    public Task Handle(ExerciseDifficultyUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseDifficultyUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewDifficulty: notification.NewDifficulty,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region ImageUrl
public sealed class ExerciseImageUrlUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseImageUrlUpdatedEvent>
{
    public Task Handle(ExerciseImageUrlUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseImageUrlUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewImageUrl: notification.NewImageUrl,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region AudioUrl
public sealed class ExerciseAudioUrlUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseAudioUrlUpdatedEvent>
{
    public Task Handle(ExerciseAudioUrlUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseAudioUrlUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewAudioUrl: notification.NewAudioUrl,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region Deleted
public sealed class ExerciseDeletedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseDeletedEvent>
{
    public Task Handle(ExerciseDeletedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseDeletedIntegrationEvent(
                ExerciseId: notification.ExerciseId
            ),
            cancellationToken);
}
#endregion
#region  Explanation
public sealed class ExerciseExplanationUpdatedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<ExerciseExplanationUpdatedEvent>
{
    public Task Handle(ExerciseExplanationUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseExplanationUpdatedIntegrationEvent(
                ExerciseId: notification.ExerciseId,
                NewExplanation: notification.NewExplanation,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
#region IncrementTotalExercisesPublished
public sealed class ExerciseIncrementTotalExercisesPublishedOutbox(ILessonOutboxWriter outboxWriter)
    : INotificationHandler<TopicTotalExercisesPublishedUpdatedEvent>
{
    public Task Handle(TopicTotalExercisesPublishedUpdatedEvent notification, CancellationToken cancellationToken)
        => outboxWriter.EnqueueAsync(
            new ExerciseIncrementTotalExercisesPublishedIntegrationEvent(
                TopicId: notification.TopicId,
                TotalExercisesPublished: notification.TotalExercisesPublished,
                UpdatedAt: notification.UpdatedAt
            ),
            cancellationToken);
}
#endregion
