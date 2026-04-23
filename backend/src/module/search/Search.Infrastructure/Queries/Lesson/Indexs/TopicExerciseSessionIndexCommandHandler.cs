using Elastic.Clients.Elasticsearch.Mapping;
namespace Search.Infrastructure.Queries.Lesson.Indexs;

public sealed record TopicExerciseSessionSnapshotInitializedCommand(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    int TotalExercises,
    int CurrentSequenceNo,
    IReadOnlyList<ExerciseSessionItemSnapshot> ExerciseItems,
    DateTime InitializedAt,
    DateTime UpdatedAt) : IRequest<Unit>;
public sealed record ExerciseSessionStartedCommand(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    DateTime UpdatedAt) : IRequest<Unit>;

#region StartedLearning
public sealed class ExerciseSessionStartedCommandHandler(
    ElasticsearchClient elasticClient,
    ILogger<ExerciseSessionStartedCommandHandler> logger) : IRequestHandler<ExerciseSessionStartedCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;
    private readonly ILogger<ExerciseSessionStartedCommandHandler> _logger = logger;

    public async Task<Unit> Handle(ExerciseSessionStartedCommand request, CancellationToken cancellationToken)
    {
        await TopicExerciseSessionIndexBootstrap.EnsureIndexExistsAsync(_elasticClient, cancellationToken);

        // vì khi bắt đầu session thì chưa có thông tin về total exercises, 
        // current sequence, exercise items nên sẽ upsert một document mới 
        // với những thông tin này là mặc định (0 hoặc empty) 
        // để đảm bảo có dữ liệu để search ngay khi session bắt đầu, 
        // sau đó khi có snapshot initialized event thì sẽ update lại với đầy đủ thông tin
        var upsertDocument = new TopicExerciseSessionSearch(
            sessionId: request.SessionId,
            userId: request.UserId,
            topicId: request.TopicId,
            totalExercises: 0,
            currentSequenceNo: 0,
            exerciseItems: [],
            initializedAt: request.UpdatedAt,
            updatedAt: request.UpdatedAt);

        var updateResponse = await _elasticClient.UpdateAsync<TopicExerciseSessionSearch, object>(
            ConstantIndexElastic.TopicExerciseSessionIndex,
            request.SessionId,
            u => u
                .Index(ConstantIndexElastic.TopicExerciseSessionIndex)
                .Doc(new { request.UpdatedAt })
                .Upsert(upsertDocument) // upsert nếu chưa có document nào cho session này, tránh trường hợp update không tìm thấy document khi event started đến trước event snapshot initialized
                .RetryOnConflict(3),
            cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to update topic exercise session {request.SessionId} with started event: {updateResponse.DebugInformation}");
        }

        _logger.LogInformation("Updated topic exercise session {SessionId} with started event for user {UserId} and topic {TopicId}", request.SessionId, request.UserId, request.TopicId);
        return Unit.Value;
    }
}
#endregion

#region SessionSnapshot
public sealed class TopicExerciseSessionSnapshotInitializedCommandHandler(
    ElasticsearchClient elasticClient,
    ILogger<TopicExerciseSessionSnapshotInitializedCommandHandler> logger) : IRequestHandler<TopicExerciseSessionSnapshotInitializedCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;
    private readonly ILogger<TopicExerciseSessionSnapshotInitializedCommandHandler> _logger = logger;

    public async Task<Unit> Handle(TopicExerciseSessionSnapshotInitializedCommand request, CancellationToken cancellationToken)
    {
        await TopicExerciseSessionIndexBootstrap.EnsureIndexExistsAsync(_elasticClient, cancellationToken);

        var document = new TopicExerciseSessionSearch(
            sessionId: request.SessionId,
            userId: request.UserId,
            topicId: request.TopicId,
            totalExercises: request.TotalExercises,
            currentSequenceNo: request.CurrentSequenceNo,
            exerciseItems: request.ExerciseItems,
            initializedAt: request.InitializedAt,
            updatedAt: request.UpdatedAt);

        var response = await _elasticClient.IndexAsync(
            document,
            i => i.Index(ConstantIndexElastic.TopicExerciseSessionIndex).Id(document.SessionId),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to index topic exercise session {request.SessionId}: {response.DebugInformation}");
        }

        _logger.LogInformation("Indexed topic exercise session {SessionId} for user {UserId} and topic {TopicId}", request.SessionId, request.UserId, request.TopicId);
        return Unit.Value;
    }
}
#endregion

#region EnsureInitIndex
internal static class TopicExerciseSessionIndexBootstrap
{
    internal static async Task EnsureIndexExistsAsync(ElasticsearchClient elasticClient, CancellationToken cancellationToken)
    {
        var existsResponse = await elasticClient.Indices.ExistsAsync(ConstantIndexElastic.TopicExerciseSessionIndex, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        // chưa có index thì tạo mới với mapping phù hợp
        var createResponse = await elasticClient.Indices.CreateAsync(ConstantIndexElastic.TopicExerciseSessionIndex, c => c
            .Mappings<TopicExerciseSessionSearch>(m => m
                .Properties(p => p
                    .Keyword(tp => tp.SessionId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.UserId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.TopicId, k => k.Fields(f => f.Keyword("keyword")))
                    .IntegerNumber(tp => tp.TotalExercises)
                    .IntegerNumber(tp => tp.CurrentSequenceNo)
                    .Nested(nameof(TopicExerciseSessionSearch.ExerciseItems), CreateExerciseItemsNestedProperty())
                    .Date(tp => tp.InitializedAt)
                    .Date(tp => tp.UpdatedAt)
                )
            ),
            cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to create index {ConstantIndexElastic.TopicExerciseSessionIndex}: {createResponse.DebugInformation}");
        }
    }

    private static NestedProperty CreateExerciseItemsNestedProperty()
    {
        return new NestedProperty
        {
            Properties = new Properties
            {
                [nameof(ExerciseSessionItemSnapshot.SessionItemId)] = new KeywordProperty(),
                [nameof(ExerciseSessionItemSnapshot.ExerciseId)] = new KeywordProperty(),
                [nameof(ExerciseSessionItemSnapshot.SequenceNo)] = new IntegerNumberProperty(),
                [nameof(ExerciseSessionItemSnapshot.OrderIndex)] = new IntegerNumberProperty(),
                [nameof(ExerciseSessionItemSnapshot.AttemptId)] = new KeywordProperty(),
                [nameof(ExerciseSessionItemSnapshot.Status)] = new KeywordProperty(),
                [nameof(ExerciseSessionItemSnapshot.ViewedAt)] = new DateProperty(),
                [nameof(ExerciseSessionItemSnapshot.AnsweredAt)] = new DateProperty()
            }
        };
    }
}
#endregion