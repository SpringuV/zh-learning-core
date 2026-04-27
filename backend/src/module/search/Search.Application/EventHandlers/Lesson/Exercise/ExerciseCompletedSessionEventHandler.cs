namespace Search.Application.EventHandlers.Lesson.Exercise;

#region CompletedSession
public class ExerciseCompletedSessionEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseCompletedSessionEventHandler> logger) 
    : IIntegrationEventHandler<UserTopicExerciseSessionCompletedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseCompletedSessionEventHandler> _logger = logger;

    public async Task HandleAsync(UserTopicExerciseSessionCompletedIntegrationEvent @event, CancellationToken ct = default!)
    {

        _logger.LogInformation("Received UserTopicExerciseSessionCompletedIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
        var request = new ExerciseSessionCompletedRequestDTO(
            SessionId: @event.SessionId,
            UserId: @event.UserId,
            TopicId: @event.TopicId,
            Status: @event.Status,
            TotalExercises: @event.TotalExercises,
            HskLevel: @event.HskLevel,
            TotalScore: @event.TotalScore,
            ScoreListening: @event.ScoreListening,
            ScoreReading: @event.ScoreReading,
            TotalCorrect: @event.TotalCorrect,
            TotalWrong: @event.TotalWrong,
            TimeSpentSeconds: @event.TimeSpentSeconds,
            CompletedAt: @event.CompletedAt
        );
        await _exerciseSearchService.CompletedExerciseSessionAsync(request, ct);
        // For now we only invalidate cache for exercise search, as the session completed event doesn't contain detailed info about which exercises are attempted and their scores, so we cannot determine which topic exercise session search cache to invalidate. In the future if we want to support more fine-grained cache invalidation for topic exercise session search, we can consider including more detailed info in the event such as which exercises are attempted and their scores.
        // await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicExerciseSessionSearch, ct);
        _logger.LogInformation("Cache invalidated for TopicExerciseSessionSearch after receiving UserTopicExerciseSessionCompletedIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
    }
}
#endregion
#region AttemptBatchScored
public class ExerciseAttemptBatchScoredEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseAttemptBatchScoredEventHandler> logger) 
    : IIntegrationEventHandler<ExerciseAttemptBatchScoredIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseAttemptBatchScoredEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseAttemptBatchScoredIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Received ExerciseAttemptBatchScoredIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
        var request = new ExerciseAttemptBatchScoredRequestDTO(
            SessionId: @event.SessionId,
            UserId: @event.UserId,
            TopicId: @event.TopicId,
            Attempts: @event.Attempts
        );
        await _exerciseSearchService.BulkIndexExerciseAttemptAsync(request, ct);
        // For now we only invalidate cache for exercise search, as the batch scored event doesn't contain detailed info about which exercises are attempted and their scores, so we cannot determine which topic exercise session search cache to invalidate. In the future if we want to support more fine-grained cache invalidation for topic exercise session search, we can consider including more detailed info in the event such as which exercises are attempted and their scores.
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicExerciseSessionSearch, ct);
        _logger.LogInformation("Cache invalidated for TopicExerciseSessionSearch after receiving ExerciseAttemptBatchScoredIntegrationEvent for SessionId: {SessionId}", @event.SessionId);
    }
}
#endregion