/*
flow in file:
    - TopicSearchAdminQueriesHandler.Handle() sẽ nhận request từ Mediator, 
        sau đó xây dựng query để tìm kiếm trong Elasticsearch
    - Đầu tiên là kiểm tra xem index có tồn tại không, nếu không thì trả về kết quả rỗng
    - Nếu index tồn tại, sẽ thực hiện tìm kiếm với các điều kiện filter và sort được xây dựng dựa trên request
    - Kết quả trả về sẽ được map sang TopicSearchItemAdminResponse và xây dựng thành SearchQueryResult để trả về cho caller
    - TopicSearchQueriesService sẽ gọi Mediator để gửi TopicSearchAdminQueries và nhận kết quả SearchQueryResult<TopicSearchItemAdminResponse> để trả về cho API controller
    - API controller sẽ nhận SearchQueryResult<TopicSearchItemAdminResponse> và trả về cho client
    - Client sẽ nhận kết quả và hiển thị danh sách topic, cùng với thông tin phân trang (hasNextPage, nextCursor) để client có thể gọi tiếp để lấy trang tiếp theo nếu cần
*/
namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record TopicSearchAdminQueries(
    string? Title = null,
    bool? IsPublished = null,
    string? TopicType = null,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null,
    int Take = 30,
    // keyset pagination sẽ dùng SearchAfter, sẽ lấy những document có CreatedAt nhỏ hơn timestamp của document cuối cùng trong page trước, để tránh việc skip nhiều document khi trang có nhiều kết quả
    string? SearchAfterValues = null, // dùng để phân trang, timestamp của document cuối cùng trong page trước, sẽ lấy những document có CreatedAt nhỏ hơn timestamp này
    TopicSortBy SortBy = TopicSortBy.CreatedAt,
    bool OrderByDescending = true
) : IRequest<SearchQueryResult<TopicSearchItemAdminResponse>>, 
    ICacheableRequest<SearchQueryResult<TopicSearchItemAdminResponse>>, 
    ICacheScopeRequest
{
    public string CacheKey =>
        $"topic-search:{Title}:{IsPublished}:{TopicType}:{StartCreatedAt:O}:{EndCreatedAt:O}:{Take}:{SearchAfterValues}:{SortBy}:{OrderByDescending}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);

    public string CacheScope => SearchCacheScopes.TopicAdminSearch;
}

public class TopicSearchAdminQueriesHandler(
    ElasticsearchClient client, 
    ILogger<TopicSearchAdminQueriesHandler> logger)
    : IRequestHandler<TopicSearchAdminQueries, SearchQueryResult<TopicSearchItemAdminResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<TopicSearchAdminQueriesHandler> _logger = logger;

    public async Task<SearchQueryResult<TopicSearchItemAdminResponse>> Handle(TopicSearchAdminQueries request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing topic search with request: {@SearchRequest}", request);
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning empty search result.", ConstantIndexElastic.TopicIndex);
                return new SearchQueryResult<TopicSearchItemAdminResponse>(
                    Total: 0,
                    Items: [],
                    HasNextPage: false,
                    NextCursor: string.Empty);
            }
            var response = await _client.SearchAsync<TopicSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.TopicIndex);
                s.Size(request.Take + 1);
                // Xây dựng query
                // bool query để filter theo các field có trong request
                s.Query(q => q.Bool(b =>
                {
                    if(!string.IsNullOrWhiteSpace(request.Title))
                    {
                        b.Filter(f => f.Match(m => m.Field(t => t.Title.Suffix("keyword")).Query(request.Title)));
                    }
                    if (request.IsPublished.HasValue)
                    {
                        // term query sẽ filter chính xác giá trị true/false của field IsPublished
                        // tránh dùng match query vì có thể bị ảnh hưởng bởi analyzer và không filter chính xác được giá trị boolean
                        b.Filter(f => f.Term(t => t.Field(t => t.IsPublished).Value(request.IsPublished.Value)));
                    }
                    if(request.TopicType != null)
                    {
                        b.Filter(f => f.Term(t => t.Field(t => t.TopicType.Suffix("keyword")).Value(request.TopicType)));
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
                Action<SortOptionsDescriptor<TopicSearch>> primarySort = request.SortBy switch
                {
                    TopicSortBy.TotalExercises => so => so.Field(f => f.Field(t => t.TotalExercises)),
                    TopicSortBy.UpdatedAt => so => so.Field(f => f.Field(t => t.UpdatedAt)),
                    TopicSortBy.ExamYear => so => so.Field(f => f.Field(t => t.ExamYear)),
                    TopicSortBy.OrderIndex => so => so.Field(f => f.Field(t => t.OrderIndex)),
                    TopicSortBy.CreatedAt => so => so.Field(f => f.Field(t => t.CreatedAt)),
                    _ => s => s.Field(f => f.Field(t => t.CreatedAt))
                };
                s.Sort(primarySort, s => s.Field(f => f.TopicId.Suffix("keyword"), SortOrder.Asc)); 
                // Secondary sort by TopicId.keyword for stable pagination

                if (!string.IsNullOrWhiteSpace(request.SearchAfterValues))
                {
                    if (SearchAfterCursorHelper.TryParseSearchAfterValues(request.SearchAfterValues, out var fieldValues))
                    {
                        s.SearchAfter(fieldValues);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid SearchAfterValues: {SearchAfterValues}", request.SearchAfterValues);
                    }
                }
            }, cancellationToken);
            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search response is invalid. Debug information: {DebugInformation}", response.DebugInformation);
                throw new Exception("Lỗi khi tìm kiếm Topic. Vui lòng thử lại sau.");
            }
            var results = response.Documents
                .Take(request.Take + 1) // Fetch one extra for pagination check
                .Select(topic => new TopicSearchItemAdminResponse(
                    Id: topic.TopicId,
                    Title: topic.Title,
                    TopicType: topic.TopicType.ToString(),
                    ExamYear: topic.ExamYear,
                    ExamCode: topic.ExamCode,
                    OrderIndex: topic.OrderIndex,
                    IsPublished: topic.IsPublished,
                    TotalExercises: topic.TotalExercises,
                    CreatedAt: topic.CreatedAt,
                    UpdatedAt: topic.UpdatedAt
                )).ToList();
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
            return BuildPagedResult(results, request.Take, totalMatched, request.SortBy);
        }   
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing topic search with request: {@SearchRequest}", request);
            throw;
        }
    }

    private static SearchQueryResult<TopicSearchItemAdminResponse> BuildPagedResult
        (List<TopicSearchItemAdminResponse> items, 
        int take, 
        long? totalMatched, 
        TopicSortBy sortBy)
    {
        var hasNextPage = items.Count > take;
        var total = totalMatched ?? items.Count;
        var nextCursor = string.Empty;
        if (hasNextPage)
        {
            items.RemoveAt(items.Count - 1); // Remove the extra item used for pagination check
            var lastDoc = items[^1]; // cú pháp ^1 để lấy phần tử cuối cùng trong list
            var sortValue = GetSortValue(lastDoc, sortBy);
            var nextCursorJson = SearchAfterCursorHelper.BuildCursor(sortValue, lastDoc.Id);
            nextCursor = nextCursorJson;
        }

        return new SearchQueryResult<TopicSearchItemAdminResponse>(
            Total: total,
            Items: items,
            HasNextPage: hasNextPage,
            NextCursor: nextCursor);
    }
    private static object GetSortValue(TopicSearchItemAdminResponse item, TopicSortBy sortBy)
    {
        return sortBy switch
        {
            TopicSortBy.TotalExercises => item.TotalExercises,
            TopicSortBy.UpdatedAt => item.UpdatedAt,
            TopicSortBy.ExamYear => item.ExamYear ?? 0,
            TopicSortBy.OrderIndex => item.OrderIndex,
            TopicSortBy.CreatedAt => item.CreatedAt,
            _ => item.CreatedAt
        };
    }
}

