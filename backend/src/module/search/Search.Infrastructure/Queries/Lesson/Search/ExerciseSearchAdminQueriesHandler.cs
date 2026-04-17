namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record ExerciseSearchAdminQueries(
    Guid TopicId,
    string? Question = null,
    bool? IsPublished = null,
    string? SkillType = null,
    string? ExerciseType = null,
    string? Difficulty = null,
    string? Context = null,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null,
    int Take = 30,
    int Page = 1,
    ExerciseSortBy SortBy = ExerciseSortBy.CreatedAt,
    bool OrderByDescending = true) : IRequest<SearchQueryResult<ExerciseSearchItemAdminResponse>>, 
    ICacheableRequest<SearchQueryResult<ExerciseSearchItemAdminResponse>>, 
    ICacheScopeRequest
{
    public string CacheScope => SearchCacheScopes.ExerciseAdminSearch;

    public string CacheKey => "exercise-s-adm:" + 
        $"{TopicId}:{Question}:" +
        $"{ExerciseType}:" +
        $"{SkillType}:" +
        $"{Context}:{StartCreatedAt:O}:{EndCreatedAt:O}:{Take}:{Page}:{SortBy}:{OrderByDescending}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(1);
}

public class ExerciseSearchAdminQueriesHandler(ElasticsearchClient client, ILogger<ExerciseSearchAdminQueriesHandler> logger) : IRequestHandler<ExerciseSearchAdminQueries, SearchQueryResult<ExerciseSearchItemAdminResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<ExerciseSearchAdminQueriesHandler> _logger = logger;

    public async Task<SearchQueryResult<ExerciseSearchItemAdminResponse>> Handle(ExerciseSearchAdminQueries request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing exercise search with request: {@SearchRequest}", request);
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.ExerciseIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning empty search result.", ConstantIndexElastic.ExerciseIndex);
                return new SearchQueryResult<ExerciseSearchItemAdminResponse>(
                    Items: [],
                    Pagination: new PaginationResponse(
                        Page: request.Page,
                        PageSize: request.Take,
                        Total: 0));
            }
            var response = await _client.SearchAsync<ExerciseSearch>(s =>
            {
                // Chỉ search trong exercise index
                s.Indices(ConstantIndexElastic.ExerciseIndex);
                s.Size(request.Take);
                if (request.Page > 1)
                {
                    s.From((request.Page - 1) * request.Take);
                }
                // Xây dựng query
                // bool query để filter theo các field có trong request
                s.Query(q => q.Bool(b =>
                {
                    // Chỉ dùng topicId.keyword để filter chính xác theo GUID
                    b.Filter(f => f.Term(t => t.Field(e => e.TopicId.Suffix("keyword")).Value(request.TopicId.ToString("D"))));
                    if (!string.IsNullOrWhiteSpace(request.Question))
                    {
                        b.Filter(f => f.Match(m => m.Field(t => t.Question).Query(request.Question)));
                    }
                    if (!string.IsNullOrWhiteSpace(request.ExerciseType))
                    {
                        // term query sẽ filter chính xác giá trị của exercise type, tránh lỗi khi parse enum hoặc filter sai kết quả không mong muốn khi dùng match query
                        b.Filter(f => f.Term(t => t.Field(t => t.ExerciseType).Value(request.ExerciseType)));
                    }
                    if (!string.IsNullOrWhiteSpace(request.SkillType))
                    {
                        b.Filter(f => f.Term(t => t.Field(t => t.SkillType).Value(request.SkillType)));
                    }
                    if (!string.IsNullOrWhiteSpace(request.Context))
                    {
                        b.Filter(f => f.Term(t => t.Field(t => t.Context).Value(request.Context)));
                    }
                    if (!string.IsNullOrWhiteSpace(request.Difficulty))
                    {
                        b.Filter(f => f.Term(t => t.Field(t => t.Difficulty).Value(request.Difficulty)));
                    }
                    if (request.IsPublished.HasValue)
                    {
                        b.Filter(f => f.Term(t => t.Field(t => t.IsPublished).Value(request.IsPublished.Value)));
                    }
                    if (request.StartCreatedAt.HasValue || request.EndCreatedAt.HasValue)
                    {
                        b.Filter(f => f.Range(r => r.Date(dr =>
                        {
                            dr.Field(t => t.CreatedAt);
                            if (request.StartCreatedAt.HasValue)
                            {
                                dr.Gte(request.StartCreatedAt.Value);
                            }
                            if (request.EndCreatedAt.HasValue)
                            {
                                dr.Lte(request.EndCreatedAt.Value);
                            }
                        })));
                    }
                }));
                var primaryOrder = request.OrderByDescending ? SortOrder.Desc : SortOrder.Asc;
                Action<SortOptionsDescriptor<ExerciseSearch>> primarySort = request.SortBy switch
                {
                    ExerciseSortBy.CreatedAt => s => s.Field(c => c.CreatedAt, primaryOrder),
                    ExerciseSortBy.UpdatedAt => s => s.Field(c => c.UpdatedAt, primaryOrder),
                    ExerciseSortBy.OrderIndex => s => s.Field(c => c.OrderIndex, primaryOrder),
                    _ => s => s.Field(c => c.CreatedAt, primaryOrder)
                };
                s.Sort(primarySort, s => s.Field(f => f.ExerciseId.Suffix("keyword"), SortOrder.Asc)); // Secondary sort by ExerciseId.keyword for stable pagination
            }, cancellationToken);
            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search response is invalid. Debug information: {DebugInformation}", response.DebugInformation);
                throw new Exception("Lỗi khi tìm kiếm bài tập. Vui lòng thử lại sau.");
            }
            var results = response.Documents
                .Take(request.Take)
                .Select(exercise => new ExerciseSearchItemAdminResponse(
                    ExerciseId: exercise.ExerciseId,
                    Question: exercise.Question,
                    ExerciseType: exercise.ExerciseType.ToString(),
                    SkillType: exercise.SkillType.ToString(),
                    Context: exercise.Context.ToString(),
                    Difficulty: exercise.Difficulty.ToString(),
                    UpdatedAt: exercise.UpdatedAt,
                    IsPublished: exercise.IsPublished,
                    OrderIndex: exercise.OrderIndex,
                    CreatedAt: exercise.CreatedAt))
                .ToList();
            var totalHits = response.HitsMetadata?.Total ?? 0;
            long? totalMatched = null; // Elasticsearch 8.0+ returns total hits 
            // as an object with value and relation, we can check if it's an exact count 
            // or a lower bound 
            if(totalHits is not null)
            {
                totalMatched = totalHits.Match(
                    hitCount => hitCount?.Value ?? 0,
                    value => value);
            }
            return BuildPagedResult(results, request.Page, request.Take, totalMatched);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing topic search with request: {@SearchRequest}", request);
            throw;
        }
    }

    private static SearchQueryResult<ExerciseSearchItemAdminResponse> BuildPagedResult(
        List<ExerciseSearchItemAdminResponse> items,
        int page,
        int take,
        long? totalMatched)
    {
        var total = totalMatched ?? items.Count;

        return new SearchQueryResult<ExerciseSearchItemAdminResponse>(
            Items: items,
            Pagination: new PaginationResponse(
                Page: page,
                PageSize: take,
                Total: total));
    }
}