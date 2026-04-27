namespace Lesson.Application.Outbox;

public sealed class ExerciseSessionCompletedOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<UserTopicExerciseSessionCompletedEvent>
{
    public Task Handle(UserTopicExerciseSessionCompletedEvent notification, CancellationToken cancellationToken)
        => lessonOutboxWriter.EnqueueAsync(
            new UserTopicExerciseSessionCompletedIntegrationEvent(
                SessionId: notification.SessionId,
                UserId: notification.UserId,
                TopicId: notification.TopicId,
                Status: notification.Status,
                TotalExercises: notification.TotalExercises,
                HskLevel: notification.HskLevel,
                TotalScore: notification.TotalScore,
                TotalCorrect: notification.TotalCorrect,
                TotalWrong: notification.TotalWrong,
                ScoreListening: notification.ScoreListening,
                ScoreReading: notification.ScoreReading,
                TimeSpentSeconds: notification.TimeSpentSeconds,
                CompletedAt: notification.CompletedAt
            ),
            cancellationToken);
}

public sealed class ExerciseAttemptBatchScoredOutbox(ILessonOutboxWriter lessonOutboxWriter)
    : INotificationHandler<ExerciseAttemptBatchScoredEvent>
{
    public Task Handle(ExerciseAttemptBatchScoredEvent notification, CancellationToken cancellationToken)
        => lessonOutboxWriter.EnqueueAsync(
            new ExerciseAttemptBatchScoredIntegrationEvent(
                SessionId: notification.SessionId,
                UserId: notification.UserId,
                TopicId: notification.TopicId,
                Attempts: notification.Attempts
            ),
            cancellationToken);
}