namespace Search.Infrastructure.Queries.Lesson.Client;

public record TopicSearchForDashboardClientQueries(string Slug)
    : IRequest<Result<IEnumerable<TopicSearchForDashboardClientResponse>>>,
    ICacheScopeRequest,
    ICacheableRequest<Result<IEnumerable<TopicSearchForDashboardClientResponse>>>
{
    public string CacheKey => $"TopicDashClient:Slug:{Slug}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(1); // cache kết quả trong 1 phút
    public string CacheScope => SearchCacheScopes.TopicPublicSearch;
}

public class TopicForDashboardClientQueriesHandler(ElasticsearchClient client, ILogger<TopicForDashboardClientQueriesHandler> logger) : IRequestHandler<TopicSearchForDashboardClientQueries, Result<IEnumerable<TopicSearchForDashboardClientResponse>>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<TopicForDashboardClientQueriesHandler> _logger = logger;

    public async Task<Result<IEnumerable<TopicSearchForDashboardClientResponse>>> Handle(TopicSearchForDashboardClientQueries request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling TopicSearchForDashboardClientQueries for Slug: {Slug}", request.Slug);

            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning empty topic dashboard result.", ConstantIndexElastic.TopicIndex);
                return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.SuccessResult([]);
            }

            var slug = request.Slug;
            var queriesCourse = _client.SearchAsync<CourseSearch>(s => s
                .Indices(ConstantIndexElastic.CourseIndex)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Slug.Suffix("keyword"))
                        .Value(slug)
                    )
                ),
                cancellationToken
            );
            var responseCourse = await queriesCourse;
            if (!responseCourse.IsValidResponse)
            {
                _logger.LogError("Invalid Elasticsearch response when loading course for slug {Slug}. DebugInfo: {DebugInfo}",
                    request.Slug,
                    responseCourse.DebugInformation);
                return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.FailureResult(
                    "An error occurred while loading topics. Please try again later.",
                    errorCode: (int)ErrorCode.INTERNAL_ERROR
                );
            }

            var queries = _client.SearchAsync<TopicSearch>(s => s
                .Indices(ConstantIndexElastic.TopicIndex)
                .Query(q => q
                    .Bool(b => b
                        .Filter(
                            f => f.Term(t => t
                                .Field(ff => ff.CourseId.Suffix("keyword"))
                                .Value(responseCourse.Documents.First().CourseId.ToString())
                            ),
                            f => f.Term(t => t
                                .Field(f => f.IsPublished)
                                .Value(true)
                            )
                        )
                    )
                )
                .Sort(s => s
                    .Field(f => f
                        .Field(ff => ff.OrderIndex)
                        .Order(SortOrder.Asc)
                    )
                ),
                cancellationToken
            );

            var response = await queries;
            if (!response.IsValidResponse)
            {
                _logger.LogError("Invalid Elasticsearch response when loading topic dashboard list for slug {Slug}. DebugInfo: {DebugInfo}",
                    request.Slug,
                    response.DebugInformation);
                return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.FailureResult(
                    "An error occurred while loading topics. Please try again later.",
                    errorCode: (int)ErrorCode.INTERNAL_ERROR
                );
            }

            var result = response.Documents.Select(doc => new TopicSearchForDashboardClientResponse(
                Id: doc.TopicId,
                Title: doc.Title,
                Description: doc.Description,
                TopicType: doc.TopicType.ToString(),
                TotalExercises: doc.TotalExercises,
                Slug: doc.Slug,
                EstimatedTimeMinutes: doc.EstimatedTimeMinutes,
                ExamYear: doc.ExamYear,
                ExamCode: doc.ExamCode,
                OrderIndex: doc.OrderIndex
            )).ToList();

            return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.SuccessResult(result);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for topics for dashboard. Details: {Message}", ex.Message);
            return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.FailureResult(
                "An error occurred while loading topics. Please try again later.",
                errorCode: (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}