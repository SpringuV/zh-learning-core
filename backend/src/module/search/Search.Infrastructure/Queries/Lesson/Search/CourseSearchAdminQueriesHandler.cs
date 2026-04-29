namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record CourseSearchAdminQueries(
    string? Title = null,
    int Take = 30,
    int Page = 1,
    bool? IsPublished = null,
    int? HskLevel = null,
    CourseSortBy SortBy = CourseSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null
): IRequest<SearchQueryResult<CourseSearchItemAdminResponse>>, 
    ICacheableRequest<SearchQueryResult<CourseSearchItemAdminResponse>>, 
    ICacheScopeRequest
{
    public string CacheKey =>
    // kí tự O trong format string để serialize DateTime theo chuẩn ISO 8601 đảm bảo cache key ổn định và chính xác khi có trường DateTime
        $"course-s-adm:{Title}:{Take}:{Page}:{SortBy}:{OrderByDescending}:{StartCreatedAt:O}:{EndCreatedAt:O}:{IsPublished}:{HskLevel}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(1);

    public string CacheScope => SearchCacheScopes.CourseAdminSearch;
}

public class CourseSearchAdminQueriesHandler(ElasticsearchClient client, ILogger<CourseSearchAdminQueriesHandler> logger) : IRequestHandler<CourseSearchAdminQueries, SearchQueryResult<CourseSearchItemAdminResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<CourseSearchAdminQueriesHandler> _logger = logger;

    public async Task<SearchQueryResult<CourseSearchItemAdminResponse>> Handle(CourseSearchAdminQueries request, CancellationToken cancellationToken)
    {
        
        try
        {
            _logger.LogDebug("Executing course search with request: {@SearchRequest}", request);
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.CourseIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning empty search result.", ConstantIndexElastic.CourseIndex);
                return new SearchQueryResult<CourseSearchItemAdminResponse>(
                    Items: [],
                    Pagination: new PaginationResponse(
                        Page: request.Page,
                        PageSize: request.Take,
                        Total: 0));
            }
            var response = await _client.SearchAsync<CourseSearch>(s =>
            {
                // Chỉ search trong course index
                s.Indices(ConstantIndexElastic.CourseIndex);
                s.Size(request.Take);
                if (request.Page > 1)
                {
                    s.From((request.Page - 1) * request.Take);
                }
                // Xây dựng query
                // bool query để phép toán search và filter cho các field trong request
                s.Query(q => q.Bool(b =>
                {
                    if (!string.IsNullOrWhiteSpace(request.Title))
                    {
                        // Title luôn phải match nếu người dùng tìm theo title.
                        b.Must(f => f.Match(m => m.Field(c => c.Title).Query(request.Title)));
                    }
                    if (request.StartCreatedAt.HasValue || request.EndCreatedAt.HasValue)
                    {
                        b.Filter(f => f.Range(r => r.Date(dr =>
                        {
                            dr.Field(c => c.CreatedAt);
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
                    if (request.HskLevel.HasValue)
                    {
                        b.Filter(f => f.Term(t => t.Field(c => c.HskLevel).Value(request.HskLevel.Value)));
                    }
                    if (request.IsPublished.HasValue)
                    {
                        b.Filter(f => f.Term(t => t.Field(c => c.IsPublished).Value(request.IsPublished.Value)));
                    }
                    
                }));
                var primaryOrder = request.OrderByDescending ? SortOrder.Desc : SortOrder.Asc;
                Action<SortOptionsDescriptor<CourseSearch>> primarySort = request.SortBy switch
                {
                    CourseSortBy.Title => s => s.Field(c => c.Title.Suffix("keyword"), primaryOrder),
                    CourseSortBy.HskLevel => s => s.Field(c => c.HskLevel, primaryOrder),
                    CourseSortBy.OrderIndex => s => s.Field(c => c.OrderIndex, primaryOrder),
                    CourseSortBy.TotalStudentsEnrolled => s => s.Field(c => c.TotalStudentsEnrolled, primaryOrder),
                    CourseSortBy.TotalTopics => s => s.Field(c => c.TotalTopics, primaryOrder),
                    CourseSortBy.CreatedAt => s => s.Field(c => c.CreatedAt, primaryOrder),
                    CourseSortBy.UpdatedAt => s => s.Field(c => c.UpdatedAt, primaryOrder),
                    _ => s => s.Field(c => c.CreatedAt, primaryOrder)
                };
                // suffix "keyword" để sort theo trường đã được keyword (không phân tích) đảm bảo thứ tự ổn định và chính xác
                s.Sort(primarySort, s => s.Field(f => f.CourseId.Suffix("keyword"), SortOrder.Asc)); // Secondary sort by CourseId.keyword for stable pagination
            }, cancellationToken);
            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search response is invalid. Debug information: {DebugInformation}", response.DebugInformation);
                throw new Exception("Lỗi khi tìm kiếm khóa học. Vui lòng thử lại sau.");
            }
            var results = response.Documents
                .Take(request.Take)
                .Select(course => new CourseSearchItemAdminResponse(
                    Id: course.CourseId,
                    Title: course.Title,
                    HskLevel: course.HskLevel,
                    OrderIndex: course.OrderIndex,
                    TotalStudentsEnrolled: course.TotalStudentsEnrolled,
                    TotalTopics: course.TotalTopics,
                    Slug: course.Slug,
                    IsPublished: course.IsPublished,
                    CreatedAt: course.CreatedAt,
                    UpdatedAt: course.UpdatedAt
                ))
                .ToList();
            var totalHits = response.HitsMetadata?.Total ?? 0;
            long? totalMatched = null;
            if (totalHits is not null)
            {

                totalMatched = totalHits.Match(hitCount => hitCount?.Value ?? 0, value => value); 
                // value là trường hợp totalHits là TotalValue (tức là khi total hits > 10k, 
                // Elasticsearch sẽ trả về total hits dạng value thay vì count)
            }
            return BuildPagedResult(results, request.Page, request.Take, totalMatched);
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching courses with request: {@SearchRequest}", request);
            throw; // Rethrow the exception after logging it
        }
    }
    private SearchQueryResult<CourseSearchItemAdminResponse> BuildPagedResult(
        List<CourseSearchItemAdminResponse> results, 
        int page,
        int take, 
        long? totalMatched = null)
    {
        _logger.LogInformation("Query executed successfully: {DocumentCount} results returned", results.Count);

        // totalMatched from response metadata is always accurate (total matching docs)
        var total = totalMatched ?? results.Count;

        return new SearchQueryResult<CourseSearchItemAdminResponse>(
            Items: results,
            Pagination: new PaginationResponse(
                Page: page,
                PageSize: take,
                Total: total));
    }
}


