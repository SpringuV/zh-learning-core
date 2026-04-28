namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record ResultCompleteSessionSearchQueries(Guid SessionId, Guid UserId) : IRequest<Result<ResultCompleteSessionResponse>>;

public class ResultCompleteSessionSearchQueriesHandler(ElasticsearchClient elasticsearchClient, ILogger<ResultCompleteSessionSearchQueriesHandler> logger) : IRequestHandler<ResultCompleteSessionSearchQueries, Result<ResultCompleteSessionResponse>>
{
    private readonly ElasticsearchClient _elasticsearchClient = elasticsearchClient;
    private readonly ILogger<ResultCompleteSessionSearchQueriesHandler> _logger = logger;

    public async Task<Result<ResultCompleteSessionResponse>> Handle(ResultCompleteSessionSearchQueries request, CancellationToken cancellationToken)
    {
        var response = await _elasticsearchClient.SearchAsync<TopicExerciseSessionSearch>(s => s
            .Indices(ConstantIndexElastic.TopicExerciseSessionIndex)
            .Size(1)
            .Query(q => q
                .Term(t => t
                    .Field(f => f.SessionId.Suffix("keyword"))
                    .Value(request.SessionId.ToString())
                )
            ), cancellationToken);
        if (!response.IsValidResponse || response.Hits.Count == 0)        {
            _logger.LogWarning("ResultCompleteSessionSearchQueriesHandler: No session found in Elasticsearch for SessionId: {SessionId}", request.SessionId);
            return Result<ResultCompleteSessionResponse>.FailureResult("Không tìm thấy kết quả phiên học.", (int)ErrorCode.NOTFOUND);
        }
        var sessionResult = response.Hits.First().Source;
        var responseDTO = new ResultCompleteSessionResponse(
            SessionId: sessionResult!.SessionId,
            UserId: sessionResult.UserId,
            TotalExercises: sessionResult.TotalExercises,
            TotalScore: sessionResult.TotalScore,
            TotalCorrect: sessionResult.TotalCorrect,
            TotalWrong: sessionResult.TotalWrong,
            ScoreListening: sessionResult.ScoreListening,
            ScoreReading: sessionResult.ScoreReading,
            TimeSpentSeconds: sessionResult.TimeSpentSeconds,
            Status: sessionResult.Status
        );

        if (sessionResult.UserId != request.UserId)
        {
            _logger.LogWarning("ResultCompleteSessionSearchQueriesHandler: UserId mismatch for SessionId: {SessionId}. Expected UserId: {ExpectedUserId}, Actual UserId: {ActualUserId}", request.SessionId, request.UserId, sessionResult.UserId);
            return Result<ResultCompleteSessionResponse>.FailureResult("Không tìm thấy kết quả phiên học.", (int)ErrorCode.NOTFOUND);
        }
        return Result<ResultCompleteSessionResponse>.SuccessResult(responseDTO);
    }
}
