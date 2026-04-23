namespace Search.Infrastructure.Queries.Lesson.Indexs;

public sealed record TopicProgressIndexCommand(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    int TotalAttempts,
    int TotalAnswered,
    int TotalCorrect,
    int TotalWrong,
    float TotalScore,
    float? AccuracyRate,
    DateTime CreatedAt) : IRequest<Unit>;

#region ProgressIndexHandler
public sealed class TopicProgressIndexCommandHandler(
    ElasticsearchClient elasticClient,
    ILogger<TopicProgressIndexCommandHandler> logger) : IRequestHandler<TopicProgressIndexCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;
    private readonly ILogger<TopicProgressIndexCommandHandler> _logger = logger;

    public async Task<Unit> Handle(TopicProgressIndexCommand request, CancellationToken cancellationToken)
    {
        await TopicProgressIndexBootstrap.EnsureIndexExistsAsync(_elasticClient, cancellationToken);

        var document = new TopicProgressSearch
        {
            TopicProgressId = request.TopicProgressId,
            UserId = request.UserId,
            TopicId = request.TopicId,
            TotalAttempts = request.TotalAttempts,
            TotalAnswered = request.TotalAnswered,
            TotalCorrect = request.TotalCorrect,
            TotalWrong = request.TotalWrong,
            TotalScore = request.TotalScore,
            AccuracyRate = request.AccuracyRate,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.CreatedAt
        };

        var response = await _elasticClient.IndexAsync(
            document,
            i => i.Index(ConstantIndexElastic.TopicProgressIndex).Id(document.TopicProgressId),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to index topic progress {request.TopicProgressId}: {response.DebugInformation}");
        }

        _logger.LogInformation("Indexed topic progress {TopicProgressId} for user {UserId} and topic {TopicId}", request.TopicProgressId, request.UserId, request.TopicId);
        return Unit.Value;
    }
}
#endregion
#region IndexBootstrap
internal static class TopicProgressIndexBootstrap
{
    internal static async Task EnsureIndexExistsAsync(ElasticsearchClient elasticClient, CancellationToken cancellationToken)
    {
        var existsResponse = await elasticClient.Indices.ExistsAsync(ConstantIndexElastic.TopicProgressIndex, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        var createResponse = await elasticClient.Indices.CreateAsync(ConstantIndexElastic.TopicProgressIndex, c => c
            .Mappings<TopicProgressSearch>(m => m
                .Properties(p => p
                    .Keyword(tp => tp.TopicProgressId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.UserId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.TopicId, k => k.Fields(f => f.Keyword("keyword")))
                    .IntegerNumber(tp => tp.TotalAttempts)
                    .IntegerNumber(tp => tp.TotalAnswered)
                    .IntegerNumber(tp => tp.TotalCorrect)
                    .IntegerNumber(tp => tp.TotalWrong)
                    .Date(tp => tp.CreatedAt)
                    .Date(tp => tp.UpdatedAt)
                )
            ),
            cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to create index {ConstantIndexElastic.TopicProgressIndex}: {createResponse.DebugInformation}");
        }
    }
}
#endregion