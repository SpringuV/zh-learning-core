using Elastic.Clients.Elasticsearch.Mapping;
using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Indexs;


public sealed record ExerciseAttemptBatchScoredSearchCommand(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    IReadOnlyList<ExerciseAttemptBatchScoredItemDTO> Attempts
) : IRequest<Unit>;

#region BatchScored
public class TopicExerciseSessionBatchScoredSearchCommandHandler(
    ElasticsearchClient elasticClient,
    ILogger<TopicExerciseSessionBatchScoredSearchCommandHandler> logger) : IRequestHandler<ExerciseAttemptBatchScoredSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;
    private readonly ILogger<TopicExerciseSessionBatchScoredSearchCommandHandler> _logger = logger;

    public async Task<Unit> Handle(ExerciseAttemptBatchScoredSearchCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ExerciseAttemptBatchScoredSearchCommand for SessionId: {SessionId}", request.SessionId);
        await TopicExerciseAttemptIndexBootstrap.EnsureIndexExistsAsync(_elasticClient, cancellationToken);

        var bulkRequest = new BulkRequest(ConstantIndexElastic.TopicExerciseAttemptIndex);
        var operations = new List<IBulkOperation>();

        foreach (var attempt in request.Attempts)
        {
            var document = new TopicExerciseAttemptSearch(
                attemptId: attempt.AttemptId,
                sessionId: request.SessionId,
                exerciseId: attempt.ExerciseId,
                userId: request.UserId,
                topicId: request.TopicId,
                answer: attempt.CorrectAnswer,
                question: attempt.Question,
                description: attempt.Description,
                explanation: attempt.Explanation,
                audioUrl: attempt.AudioUrl,
                imageUrl: attempt.ImageUrl,
                exerciseType: attempt.ExerciseType,
                skillType: attempt.SkillType,
                difficulty: attempt.Difficulty,
                correctAnswer: attempt.CorrectAnswer,
                options: attempt.Options,
                isCorrect: attempt.IsCorrect,
                score: attempt.Score,
                attemptedAt: attempt.UpdatedAt
            );

            operations.Add(new BulkIndexOperation<TopicExerciseAttemptSearch>(document)
            {
                Id = attempt.AttemptId
            });
        }

        bulkRequest.Operations = operations;
        var response = await _elasticClient.BulkAsync(bulkRequest, cancellationToken);

        if (!response.Errors)
        {
            _logger.LogInformation("Exercise session batch scored and indexed attempts for SessionId: {SessionId}", request.SessionId);
            return Unit.Value;
        }

        var failedItems = response.ItemsWithErrors
            .Select(x => x.Error?.Reason)
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .ToList();
        var reasonText = failedItems.Count > 0
            ? string.Join("; ", failedItems)
            : (response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation);

        throw new InvalidOperationException($"Failed to index topic exercise attempts for session {request.SessionId}. Errors: {reasonText}");
    }
}
#endregion

internal static class TopicExerciseAttemptIndexBootstrap
{
    internal static async Task EnsureIndexExistsAsync(ElasticsearchClient elasticClient, CancellationToken cancellationToken)
    {
        var existsResponse = await elasticClient.Indices.ExistsAsync(ConstantIndexElastic.TopicExerciseAttemptIndex, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        var createResponse = await elasticClient.Indices.CreateAsync(ConstantIndexElastic.TopicExerciseAttemptIndex, c => c
            .Mappings<TopicExerciseAttemptSearch>(m => m
                .Properties(p => p
                    .Keyword(tp => tp.AttemptId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.SessionId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.ExerciseId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.UserId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.TopicId, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.SkillType, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.ExerciseType, k => k.Fields(f => f.Keyword("keyword")))
                    .Keyword(tp => tp.Difficulty, k => k.Fields(f => f.Keyword("keyword")))
                    .Text(tp => tp.Question)
                    .Text(tp => tp.Description)
                    .Text(tp => tp.Explanation)
                    .Keyword(tp => tp.CorrectAnswer)
                    .Keyword(tp => tp.AudioUrl)
                    .Keyword(tp => tp.ImageUrl)
                    .Text(tp => tp.Answer)
                    .Boolean(tp => tp.IsCorrect)
                    .FloatNumber(tp => tp.Score)
                    .Date(tp => tp.AttemptedAt)
                    // Create nested mapping for Options property
                    .Nested(nameof(TopicExerciseAttemptSearch.Options), CreateOptionsNestedProperty())
                )
            ),
            cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to create index {ConstantIndexElastic.TopicExerciseAttemptIndex}: {createResponse.DebugInformation}");
        }
    }

    private static NestedProperty CreateOptionsNestedProperty()
    {
        return new NestedProperty
        {
            Properties = new Properties
            {
                [nameof(ExerciseOption.Id)] = new KeywordProperty(),
                [nameof(ExerciseOption.Text)] = new TextProperty()
            }
        };
    }
}
