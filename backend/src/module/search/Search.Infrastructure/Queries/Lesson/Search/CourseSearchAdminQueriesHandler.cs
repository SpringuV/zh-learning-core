namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record CourseSearchAdminQueries(
    string? Title = null,
    int Take = 30,
    // Keyset pagination: JSON array của [sortValue, userId]
    // Ví dụ: "[5, \"user-123\"]" khi sort by CurrentLevel
    // Ensures stable pagination khi sort field thay đổi
    string? SearchAfterValues = null,
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
        $"course-search:{Title}:{Take}:{SearchAfterValues}:{SortBy}:{OrderByDescending}:{StartCreatedAt:O}:{EndCreatedAt:O}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);

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
                    Total: 0,
                    Items: [],
                    HasNextPage: false,
                    NextCursor: string.Empty);
            }
            var response = await _client.SearchAsync<CourseSearch>(s =>
            {
                // Chỉ search trong course index
                s.Indices(ConstantIndexElastic.CourseIndex);
                // Lấy thêm 1 kết quả để kiểm tra xem có trang tiếp theo hay không
                s.Size(request.Take + 1);
                // Xây dựng query
                // bool query để filter theo các field có trong request
                s.Query(q => q.Bool(b =>
                {
                    if (!string.IsNullOrWhiteSpace(request.Title))
                    {
                        b.Filter(f => f.Match(m => m.Field(c => c.Title.Suffix("keyword")).Query(request.Title)));
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
                if (!string.IsNullOrWhiteSpace(request.SearchAfterValues))
                {
                    if (SearchAfterCursorHelper.TryParseSearchAfterValues(request.SearchAfterValues, out var fieldValues))
                    {
                        s.SearchAfter(fieldValues);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid SearchAfterValues format: {SearchAfterValues}", request.SearchAfterValues);
                    }
                }
            }, cancellationToken);
            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search response is invalid. Debug information: {DebugInformation}", response.DebugInformation);
                throw new Exception("Lỗi khi tìm kiếm khóa học. Vui lòng thử lại sau.");
            }
            var results = response.Documents
                .Take(request.Take + 1) // Fetch one extra for pagination check
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
            return BuildPagedResult(results, request.Take, totalMatched, request.SortBy);
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching courses with request: {@SearchRequest}", request);
            throw; // Rethrow the exception after logging it
        }
    }
    private SearchQueryResult<CourseSearchItemAdminResponse> BuildPagedResult(
        List<CourseSearchItemAdminResponse> results, 
        int take, 
        long? totalMatched = null, 
        CourseSortBy sortBy = CourseSortBy.CreatedAt)
    {
        _logger.LogInformation("Query executed successfully: {DocumentCount} results returned", results.Count);

        // hasNextPage: check if we have more docs than take (We fetched take + 1)
        var hasNextPage = results.Count > take;
        
        // totalMatched from response metadata is always accurate (total matching docs)
        var total = totalMatched ?? results.Count;

        var nextCursor = string.Empty;
        if (hasNextPage)
        {
            results.RemoveAt(results.Count - 1); // Remove the extra doc used for pagination check
            var lastDoc = results[^1]; // ^1 is the last element in the list
            var sortValue = GetSortValue(lastDoc, sortBy);
            var cursorJson = SearchAfterCursorHelper.BuildCursor(sortValue, lastDoc.Id);
            nextCursor = cursorJson;
        }

        return new SearchQueryResult<CourseSearchItemAdminResponse>(
            Total: total,
            Items: results,
            HasNextPage: hasNextPage,
            NextCursor: nextCursor);
    }

    private static object GetSortValue(CourseSearchItemAdminResponse course, CourseSortBy sortBy)
    {
        return sortBy switch
        {
            CourseSortBy.Title => course.Title,
            CourseSortBy.HskLevel => course.HskLevel,
            CourseSortBy.OrderIndex => course.OrderIndex,
            CourseSortBy.TotalStudentsEnrolled => course.TotalStudentsEnrolled,
            CourseSortBy.TotalTopics => course.TotalTopics,
            CourseSortBy.CreatedAt => course.CreatedAt,
            CourseSortBy.UpdatedAt => course.UpdatedAt,
            _ => course.CreatedAt
        };
    }
}


