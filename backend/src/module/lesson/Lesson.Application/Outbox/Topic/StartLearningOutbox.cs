namespace Lesson.Application.Outbox.Topic;

#region  CreatedTopicProgress
public sealed class TopicProgressCreatedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<UserTopicProgressCreatedEvent>
{
    public Task Handle(UserTopicProgressCreatedEvent notification, CancellationToken cancellationToken)
        => lessonOutboxWriter.EnqueueAsync(
            new UserTopicProgressCreatedIntegrationEvent(
                TopicProgressId: notification.TopicProgressId,
                TotalAnswered: notification.TotalAnswered,
                TotalAttempts: notification.TotalAttempts,
                TotalCorrect: notification.TotalCorrect,
                TotalWrong: notification.TotalWrong,
                TotalScore: notification.TotalScore,
                AccuracyRate: notification.AccuracyRate,
                UserId: notification.UserId,
                TopicId: notification.TopicId,
                CreatedAt: notification.CreatedAt
            ),
            cancellationToken);
}
#endregion
#region StartedTopicExerciseSession
public sealed class TopicExerciseSessionStartedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<UserTopicExerciseSessionStartedEvent>
{
    public Task Handle(UserTopicExerciseSessionStartedEvent notification, CancellationToken cancellationToken)
    => lessonOutboxWriter.EnqueueAsync(
        new UserTopicExerciseSessionStartedIntegrationEvent(
            SessionId: notification.SessionId,
            UserId: notification.UserId,
            TopicId: notification.TopicId,
            StartedAt: notification.StartedAt,
            HskLevel: notification.HskLevel,
            Status: notification.Status.ToString()
        ),
        cancellationToken);
}
#endregion
#region SnapshotInitialized
public sealed class TopicExerciseSessionSnapshotInitializedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<UserTopicExerciseSessionSnapshotInitializedEvent>
{
    public Task Handle(UserTopicExerciseSessionSnapshotInitializedEvent notification, CancellationToken cancellationToken)
    => lessonOutboxWriter.EnqueueAsync(
        new UserTopicExerciseSessionSnapshotInitializedIntegrationEvent(
            SessionId: notification.SessionId,
            UserId: notification.UserId,
            HskLevel: notification.HskLevel,
            TopicId: notification.TopicId,
            TotalExercises: notification.TotalExercises,
            CurrentSequenceNo: notification.CurrentSequenceNo,
            SessionItems: [.. notification.SessionItems
                .Select(item => new UserTopicExerciseSessionItemSnapshotIntegrationEvent(
                    SessionItemId: item.SessionItemId,
                    ExerciseId: item.ExerciseId,
                    SequenceNo: item.SequenceNo,
                    OrderIndex: item.OrderIndex,
                    AttemptId: item.AttemptId,
                    Status: item.Status.ToString(),
                    ViewedAt: item.ViewedAt,
                    AnsweredAt: item.AnsweredAt))],
            InitializedAt: notification.InitializedAt,
            UpdatedAt: notification.UpdatedAt
        ),
        cancellationToken);
}
#endregion
#region UpdatedTopicExerciseSessionSequence
public sealed class TopicExerciseSessionSequenceUpdatedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<UserTopicExerciseSessionSequenceUpdatedEvent>
{
    public Task Handle(UserTopicExerciseSessionSequenceUpdatedEvent notification, CancellationToken cancellationToken)
    => lessonOutboxWriter.EnqueueAsync(
        new UserTopicExerciseSessionSequenceUpdatedIntegrationEvent(
            SessionId: notification.SessionId,
            UserId: notification.UserId,
            TopicId: notification.TopicId,
            NewCurrentSequenceNo: notification.NewCurrentSequenceNo,
            UpdatedAt: notification.UpdatedAt
        ),
        cancellationToken);
}
#endregion