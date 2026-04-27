using Elastic.Clients.Elasticsearch.Core.Bulk;
using HanziAnhVu.Shared.Domain;
using Search.Infrastructure.Queries.Lesson.Indexs;

namespace Search.Infrastructure.Queries.Lesson.Update.Topic;

#region Commands
public sealed record ExerciseSessionCompletedSearchCommand(
    Guid SessionId,
    Guid UserId,
    Guid TopicId,
    ExerciseSessionStatus Status,
    int TotalExercises,
    int HskLevel,
    float TotalScore,
    int ScoreListening,
    int ScoreReading,
    int TotalCorrect,
    int TotalWrong,
    int TimeSpentSeconds,
    DateTime CompletedAt
) : IRequest<Unit>;



#endregion
#region CompletedSession
public class TopicExerciseSessionCompleteSearchCommandHandler(
    ElasticsearchClient elasticClient,
    ILogger<TopicExerciseSessionCompleteSearchCommandHandler> logger) : IRequestHandler<ExerciseSessionCompletedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;
    private readonly ILogger<TopicExerciseSessionCompleteSearchCommandHandler> _logger = logger;

    public async Task<Unit> Handle(ExerciseSessionCompletedSearchCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ExerciseSessionCompletedSearchCommand for SessionId: {SessionId}", request.SessionId);
        // update the topic exercise session document in elasticsearch with the completed session info such as status, total score, total correct, total wrong, time spent, etc. so that we can use this info to show in the topic exercise session search result and also use it for analytics
        var response = await _elasticClient.UpdateAsync<TopicExerciseSessionSearch, object>(
            ConstantIndexElastic.TopicExerciseSessionIndex,
            request.SessionId,
            u => u.Doc(new
            {
                request.UserId,
                request.TopicId,
                request.Status,
                request.TotalExercises,
                request.HskLevel,
                request.TotalScore,
                request.ScoreListening,
                request.ScoreReading,
                request.TotalCorrect,
                request.TotalWrong,
                request.TimeSpentSeconds,
                UpdatedAt = request.CompletedAt
            }).RetryOnConflict(3),
            cancellationToken
        );
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update topic exercise session with id {request.SessionId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        _logger.LogInformation("Exercise session completed and updated in Elasticsearch for SessionId: {SessionId}", request.SessionId);
        return Unit.Value;
    }
}
#endregion

