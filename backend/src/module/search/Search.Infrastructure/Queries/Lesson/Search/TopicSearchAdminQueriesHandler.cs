/*
flow in file:
    - TopicSearchAdminQueriesHandler.Handle() sẽ nhận request từ Mediator, 
        sau đó xây dựng query để tìm kiếm trong Elasticsearch
    - 1: kiểm tra xem index có tồn tại không, nếu không thì trả về kết quả rỗng
    - 2: Nếu index tồn tại, sẽ thực hiện tìm kiếm với các điều kiện filter và sort được xây dựng dựa trên request
    - 3: Kết quả trả về sẽ được map sang TopicSearchItemAdminResponse và xây dựng thành SearchQueryResult để trả về cho caller
    - 4: TopicSearchQueriesService sẽ gọi Mediator để gửi TopicSearchAdminQueries và nhận kết quả SearchQueryResult<TopicSearchItemAdminResponse> để trả về cho API controller
*/
using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record TopicSearchAdminQueries(
    Guid CourseId,
    string? Title = null,
    bool? IsPublished = null,
    TopicType? TopicType = null,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null,
    int Take = 30,
    int Page = 1,
    TopicSortBy SortBy = TopicSortBy.CreatedAt,
    bool OrderByDescending = true
) : IRequest<SearchQueryResult<TopicSearchItemAdminResponse>>, 
    ICacheableRequest<SearchQueryResult<TopicSearchItemAdminResponse>>, 
    ICacheScopeRequest
{
    public string CacheKey =>
        $"topic-s-adm:{CourseId}:{Title}:{IsPublished}:{TopicType}:{StartCreatedAt:O}:{EndCreatedAt:O}:{Take}:{Page}:{SortBy}:{OrderByDescending}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(1);

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
                    Items: [],
                    Pagination: new PaginationResponse(
                        Page: request.Page,
                        PageSize: request.Take,
                        Total: 0));
            }
            var response = await _client.SearchAsync<TopicSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.TopicIndex);
                s.Size(request.Take);
                if (request.Page > 1)
                {
                    s.From((request.Page - 1) * request.Take);
                }
                // Xây dựng query
                // bool query để filter theo các field có trong request
                s.Query(q => q.Bool(b =>
                {
                    // load courseId filter vào query để chỉ search trong topic của course đó
                    b.Must(f => f.Term(t => t.Field(t => t.CourseId.Suffix("keyword")).Value(request.CourseId.ToString("D"))));
                    if(!string.IsNullOrWhiteSpace(request.Title))
                    {
                        b.Must(f => f.Match(m => m.Field(t => t.Title).Query(request.Title)));
                    }
                    if (request.IsPublished.HasValue)
                    {
                        // term query sẽ filter chính xác giá trị true/false của field IsPublished
                        // tránh dùng match query vì có thể bị ảnh hưởng bởi analyzer và không filter chính xác được giá trị boolean
                        b.Filter(f => f.Term(t => t.Field(t => t.IsPublished).Value(request.IsPublished.Value)));
                    }
                    if(request.TopicType.HasValue)
                    {
                        b.Filter(f => f.Term(t => t.Field(t => t.TopicType.Suffix("keyword")).Value(request.TopicType.Value.ToString())));
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
                    TopicSortBy.TotalExercises => so => so.Field(f => f.Field(t => t.TotalExercises).Order(primaryOrder)),
                    TopicSortBy.UpdatedAt => so => so.Field(f => f.Field(t => t.UpdatedAt).Order(primaryOrder)),
                    TopicSortBy.ExamYear => so => so.Field(f => f.Field(t => t.ExamYear).Order(primaryOrder)),
                    TopicSortBy.OrderIndex => so => so.Field(f => f.Field(t => t.OrderIndex).Order(primaryOrder)),
                    TopicSortBy.CreatedAt => so => so.Field(f => f.Field(t => t.CreatedAt).Order(primaryOrder)),
                    _ => s => s.Field(f => f.Field(t => t.CreatedAt).Order(primaryOrder))
                };
                s.Sort(primarySort, s => s.Field(f => f.TopicId.Suffix("keyword"), SortOrder.Asc)); 
                // Secondary sort by TopicId.keyword for stable pagination
            }, cancellationToken);
            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch search response is invalid. Debug information: {DebugInformation}", response.DebugInformation);
                throw new Exception("Lỗi khi tìm kiếm Topic. Vui lòng thử lại sau.");
            }
            var results = response.Documents
                .Take(request.Take)
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
            return BuildPagedResult(results, request.Page, request.Take, totalMatched);
        }   
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing topic search with request: {@SearchRequest}", request);
            throw;
        }
    }

    private static SearchQueryResult<TopicSearchItemAdminResponse> BuildPagedResult
        (List<TopicSearchItemAdminResponse> items, 
        int page,
        int take, 
        long? totalMatched)
    {
        var total = totalMatched ?? items.Count;

        return new SearchQueryResult<TopicSearchItemAdminResponse>(
            Items: items,
            Pagination: new PaginationResponse(
                Page: page,
                PageSize: take,
                Total: total));
    }
}

