namespace Search.Application.EventHandlers.Lesson;

#region TopicProgressCreated
public class TopicProgressCreatedEventHandler(
    ITopicProgressQueriesService topicProgressQueriesService,
    ILogger<TopicProgressCreatedEventHandler> logger) : IIntegrationEventHandler<UserTopicProgressCreatedIntegrationEvent>
{
    private readonly ITopicProgressQueriesService _topicProgressQueriesService = topicProgressQueriesService;
    private readonly ILogger<TopicProgressCreatedEventHandler> _logger = logger;

    public async Task HandleAsync(UserTopicProgressCreatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Handling TopicProgressCreatedIntegrationEvent for SessionId: {SessionId}, UserId: {UserId}, TopicId: {TopicId}", @event.TopicProgressId, @event.UserId, @event.TopicId);
        var request = new TopicProgressCreatedQueriesRequest(
            TopicProgressId: @event.TopicProgressId,
            UserId: @event.UserId,
            TopicId: @event.TopicId,
            CreatedAt: @event.CreatedAt,
            TotalAnswered: @event.TotalAnswered,
            TotalCorrect: @event.TotalCorrect,
            TotalScore: @event.TotalScore,
            TotalWrong: @event.TotalWrong,
            TotalAttempts: @event.TotalAttempts,
            AccuracyRate: @event.AccuracyRate
        );

        await _topicProgressQueriesService.HandleStartedLearningAsync(request, ct);
        _logger.LogInformation("Finished handling TopicProgressCreatedIntegrationEvent for SessionId: {SessionId}", @event.TopicProgressId);
    }
}
#endregion
#region ExerciseSessionStarted
public class TopicExerciseSessionStartedEventHandler(
    ITopicProgressQueriesService topicProgressQueriesService,
    ILogger<TopicExerciseSessionStartedEventHandler> logger) : IIntegrationEventHandler<UserTopicExerciseSessionStartedIntegrationEvent>
{
    private readonly ITopicProgressQueriesService _topicProgressQueriesService = topicProgressQueriesService;
    private readonly ILogger<TopicExerciseSessionStartedEventHandler> _logger = logger;

    public async Task HandleAsync(UserTopicExerciseSessionStartedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Handling TopicExerciseSessionStartedIntegrationEvent for SessionId: {SessionId}, UserId: {UserId}, TopicId: {TopicId}", @event.SessionId, @event.UserId, @event.TopicId);
        var request = new ExerciseSessionStartedQueriesRequest(
            SessionId: @event.SessionId,
            UserId: @event.UserId,
            TopicId: @event.TopicId,
            HskLevel: @event.HskLevel,
            StartedAt: @event.StartedAt,
            Status: Enum.Parse<StatusTopicForDashboardClient>(@event.Status)
        );

        await _topicProgressQueriesService.HandleExerciseSessionStartedAsync(request, ct);
        _logger.LogInformation("Finished handling TopicExerciseSessionStartedIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
    }
}
#endregion
#region ExerciseSessionSnapshotInitialized
public class TopicExerciseSessionSnapshotInitializedEventHandler(
    ITopicProgressQueriesService topicProgressQueriesService,
    ILogger<TopicExerciseSessionSnapshotInitializedEventHandler> logger) : IIntegrationEventHandler<UserTopicExerciseSessionSnapshotInitializedIntegrationEvent>
{
    private readonly ITopicProgressQueriesService _topicProgressQueriesService = topicProgressQueriesService;
    private readonly ILogger<TopicExerciseSessionSnapshotInitializedEventHandler> _logger = logger;

    public async Task HandleAsync(UserTopicExerciseSessionSnapshotInitializedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Handling TopicExerciseSessionSnapshotInitializedIntegrationEvent for SessionId: {SessionId}, UserId: {UserId}, TopicId: {TopicId}", @event.SessionId, @event.UserId, @event.TopicId);
        var request = new ExerciseSessionSnapshotInitializedQueriesRequest(
            SessionId: @event.SessionId,
            UserId: @event.UserId,
            TopicId: @event.TopicId,
            TotalExercises: @event.TotalExercises,
            CurrentSequenceNo: @event.CurrentSequenceNo,
            HskLevel: @event.HskLevel,
            SessionItems: [.. @event.SessionItems.Select(item => new ExerciseSessionItemSnapshot
            (
                SessionItemId: item.SessionItemId,
                ExerciseId: item.ExerciseId,
                SequenceNo: item.SequenceNo,
                OrderIndex: item.OrderIndex,
                AttemptId: item.AttemptId,
                Status: Enum.Parse<ExerciseSessionItemStatus>(item.Status),
                ViewedAt: item.ViewedAt,
                AnsweredAt: item.AnsweredAt
            ))],
            InitializedAt: @event.InitializedAt,
            UpdatedAt: @event.UpdatedAt
        );

        await _topicProgressQueriesService.HandleExerciseSessionSnapshotInitializedAsync(request, ct);
        _logger.LogInformation("Finished handling TopicExerciseSessionSnapshotInitializedIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
    }
}
#endregion

#region ExerciseSessionSequenceUpdated
public class TopicExerciseSessionSequenceUpdatedEventHandler(
    ITopicProgressQueriesService topicProgressQueriesService,
    ILogger<TopicExerciseSessionSequenceUpdatedEventHandler> logger) : IIntegrationEventHandler<UserTopicExerciseSessionSequenceUpdatedIntegrationEvent>
{
    private readonly ITopicProgressQueriesService _topicProgressQueriesService = topicProgressQueriesService;
    private readonly ILogger<TopicExerciseSessionSequenceUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(UserTopicExerciseSessionSequenceUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Handling TopicExerciseSessionSequenceUpdatedIntegrationEvent for SessionId: {SessionId}, UserId: {UserId}, TopicId: {TopicId}", @event.SessionId, @event.UserId, @event.TopicId);
        var request = new ExerciseSessionSequenceUpdatedQueriesRequest(
            SessionId: @event.SessionId,
            UserId: @event.UserId,
            TopicId: @event.TopicId,
            NewCurrentSequenceNo: @event.NewCurrentSequenceNo,
            UpdatedAt: @event.UpdatedAt
        );

        await _topicProgressQueriesService.HandleUpdatedSequenceNoAsync(request, ct);
        _logger.LogInformation("Finished handling TopicExerciseSessionSequenceUpdatedIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
    }
}
#endregion