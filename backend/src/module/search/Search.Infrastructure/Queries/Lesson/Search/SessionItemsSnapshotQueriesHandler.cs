namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record ExerciseSessionItemsSnapshotSearchQueries(Guid SessionId, string SlugTopic, Guid UserId)
    : IRequest<Result<ExerciseSessionItemsSnapshotResponse>>, ICacheableRequest<ExerciseSessionItemsSnapshotResponse>, ICacheScopeRequest
{
    public string CacheScope => SearchCacheScopes.ExerciseSessionItemsSnapshot;

    public string CacheKey => $"ExerciseSessionItemsSnapshot:SessionId={SessionId}:SlugTopic={SlugTopic}:UserId={UserId}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}

public class ExerciseSessionItemsSnapshotQueriesHandler(ElasticsearchClient client, ILogger<ExerciseSessionItemsSnapshotQueriesHandler> logger) : IRequestHandler<ExerciseSessionItemsSnapshotSearchQueries, Result<ExerciseSessionItemsSnapshotResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<ExerciseSessionItemsSnapshotQueriesHandler> _logger = logger;

    public async Task<Result<ExerciseSessionItemsSnapshotResponse>> Handle(ExerciseSessionItemsSnapshotSearchQueries request, CancellationToken cancellationToken)
    {
        try
        {
            // tìm bằng sessionId trước
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicExerciseSessionIndex, cancellationToken);
            if (!indexExistsResponse.Exists)            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning not found result.", ConstantIndexElastic.TopicExerciseSessionIndex);
                return Result<ExerciseSessionItemsSnapshotResponse>.FailureResult("Exercise session items snapshot not found", (int)ErrorCode.NOTFOUND);
            }

            var responseSession = await _client.SearchAsync<TopicExerciseSessionSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.TopicExerciseSessionIndex);
                s.Size(1);
                s.Query(q => q
                    .Term(t => t
                        .Field(f => f.SessionId.Suffix("keyword"))
                        .Value(request.SessionId.ToString())
                    )
                );
            }, cancellationToken);

            if (!responseSession.IsValidResponse || responseSession.Hits.Count == 0)
            {
                _logger.LogInformation("Exercise session with ID {SessionId} not found in index. Returning not found result.", request.SessionId);
                return Result<ExerciseSessionItemsSnapshotResponse>.FailureResult("Exercise session items snapshot not found", (int)ErrorCode.NOTFOUND);
            }
            var session = responseSession.Hits.First().Source;
            if (session is null)
            {
                // fallback tìm bằng userId + slug topic + session đang active (chưa có session nào active thì trả về not found luôn)
                _logger.LogInformation("No exercise session found with SessionId {SessionId}. Attempting fallback search by UserId {UserId} and SlugTopic {SlugTopic}.", request.SessionId, request.UserId, request.SlugTopic);
                // tìm topic theo slug để lấy topicId
                var topicResponse = await _client.SearchAsync<TopicSearch>(s =>
                {
                    s.Indices(ConstantIndexElastic.TopicIndex);
                    s.Size(1);
                    s.Query(q => q
                        .Term(t => t
                            .Field(f => f.Slug.Suffix("keyword"))
                            .Value(request.SlugTopic)
                        )
                    );
                }, cancellationToken);
                var topic = topicResponse.Hits.FirstOrDefault()?.Source;
                if (topic is null)                {
                    _logger.LogInformation("Topic with slug {SlugTopic} not found in index. Returning not found result.", request.SlugTopic);
                    return Result<ExerciseSessionItemsSnapshotResponse>.FailureResult("Exercise session items snapshot not found", (int)ErrorCode.NOTFOUND);
                }
                var topicId = topic.TopicId;
                // tìm session active theo userId + topicId
                responseSession = await _client.SearchAsync<TopicExerciseSessionSearch>(s =>
                {
                    s.Indices(ConstantIndexElastic.TopicExerciseSessionIndex);
                    s.Size(1);
                    s.Query(q => q
                        .Bool(b => b
                            .Must(
                                mu => mu.Term(t => t.Field(f => f.UserId.Suffix("keyword")).Value(request.UserId.ToString())),
                                mu => mu.Term(t => t.Field(f => f.TopicId.Suffix("keyword")).Value(topicId.ToString())),
                                mu => mu.Term(t => t.Field(f => f.Status.Suffix("keyword")).Value(StatusTopicForDashboardClient.InProgress.ToString()))
                            )
                        )
                    );
                }, cancellationToken);
                if (!responseSession.IsValidResponse || responseSession.Hits.Count == 0)
                {
                    _logger.LogInformation("No active exercise session found for UserId {UserId} and TopicId {TopicId}. Returning not found result.", request.UserId, topicId);
                    return Result<ExerciseSessionItemsSnapshotResponse>.FailureResult("Exercise session items snapshot not found", (int)ErrorCode.NOTFOUND);
                }
                var sessionFallback = responseSession.Hits.First().Source;
                if (sessionFallback is null && session is null)
                {
                    _logger.LogInformation("Exercise session with UserId {UserId} and TopicId {TopicId} has null source in index. Returning not found result.", request.UserId, topicId);
                    return Result<ExerciseSessionItemsSnapshotResponse>.FailureResult("Exercise session items snapshot not found", (int)ErrorCode.NOTFOUND);
                }
                return Result<ExerciseSessionItemsSnapshotResponse>.SuccessResult(BuildExerciseSessionItemsSnapshotResponse(sessionFallback!));
            }
            return Result<ExerciseSessionItemsSnapshotResponse>.SuccessResult(BuildExerciseSessionItemsSnapshotResponse(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling ExerciseSessionItemsSnapshotSearchQueries with SessionId: {SessionId}, SlugTopic: {SlugTopic}, UserId: {UserId}", request.SessionId, request.SlugTopic, request.UserId);
            return Result<ExerciseSessionItemsSnapshotResponse>.FailureResult("An error occurred while retrieving exercise session items snapshot", (int)ErrorCode.INTERNAL_ERROR);
        }
    }

    #region Mapping
    private static ExerciseSessionItemsSnapshotResponse BuildExerciseSessionItemsSnapshotResponse(TopicExerciseSessionSearch session)
    {
        return new ExerciseSessionItemsSnapshotResponse(
            SessionId: session.SessionId,
            TotalExercises: session.TotalExercises,
            CurrentSequenceNo: session.CurrentSequenceNo,
            SessionItems: [.. session.ExerciseItems.Select(item => new BaseExerciseLearningSessionItemsDTO(
                SessionItemId: item.SessionItemId,
                ExerciseId: item.ExerciseId,
                SequenceNo: item.SequenceNo,
                OrderIndex: item.OrderIndex,
                AttemptId: item.AttemptId,
                Status: item.Status.ToString(),
                ViewedAt: item.ViewedAt,
                AnsweredAt: item.AnsweredAt
            ))]
        );
    }
    #endregion
}