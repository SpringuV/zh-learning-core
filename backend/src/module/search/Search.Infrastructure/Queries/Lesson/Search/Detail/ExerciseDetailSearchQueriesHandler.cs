namespace Search.Infrastructure.Queries.Lesson.Search.Detail;

public record ExerciseSearchDetailAdminQueries(Guid ExerciseId)
    : IRequest<ExerciseSearchDetailResponse>, ICacheableRequest<ExerciseSearchDetailResponse>, ICacheScopeRequest
{
    public string CacheScope => SearchCacheScopes.ExerciseAdminSearch;

    public string CacheKey => $"exercise-detail-ad-{ExerciseId}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(15);
}

public class ExerciseSearchDetailAdminQueriesHandler(ElasticsearchClient elasticClient) : IRequestHandler<ExerciseSearchDetailAdminQueries, ExerciseSearchDetailResponse>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<ExerciseSearchDetailResponse> Handle(ExerciseSearchDetailAdminQueries request, CancellationToken cancellationToken)
    {
        var respone = await _elasticClient.GetAsync<ExerciseSearch>(request.ExerciseId, idx => idx.Index(ConstantIndexElastic.ExerciseIndex), cancellationToken);
        if (!respone.Found || respone.Source == null)        {
            throw new KeyNotFoundException($"Exercise with ID {request.ExerciseId} not found in search index.");
        }
        var exercise = respone.Source;
        return new ExerciseSearchDetailResponse(
            ExerciseId: exercise.ExerciseId,
            ExerciseType: exercise.ExerciseType.ToString(),
            SkillType: exercise.SkillType.ToString(),
            Context: exercise.Context.ToString(),
            Question: exercise.Question,
            Slug: exercise.Slug,
            Description: exercise.Description,
            CreatedAt: exercise.CreatedAt,
            UpdatedAt: exercise.UpdatedAt,
            Difficulty: exercise.Difficulty.ToString(),
            AudioUrl: exercise.AudioUrl,
            ImageUrl: exercise.ImageUrl,
            Explanation: exercise.Explanation,
            IsPublished: exercise.IsPublished,
            OrderIndex: exercise.OrderIndex,
            Options: exercise.Options,
            CorrectAnswer: exercise.CorrectAnswer
        );
    }
}
        