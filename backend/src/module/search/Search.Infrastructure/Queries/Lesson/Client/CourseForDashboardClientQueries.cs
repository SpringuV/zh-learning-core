namespace Search.Infrastructure.Queries.Lesson.Client;

public sealed record CourseSearchForDashboardClientQueries() 
    : IRequest<Result<IEnumerable<CourseSearchForDashboardClientResponse>>>, 
    ICacheScopeRequest, 
    ICacheableRequest<Result<IEnumerable<CourseSearchForDashboardClientResponse>>>
{
    public string CacheKey => $"CourseSearchForDashboardClient:All";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10); // cache kết quả trong 10 phút

    public string CacheScope => SearchCacheScopes.CoursePublicSearch;
};

public class CourseSearchForDashboardClientQueriesHandler(ElasticsearchClient client, ILogger<CourseSearchForDashboardClientQueriesHandler> logger) : IRequestHandler<CourseSearchForDashboardClientQueries, Result<IEnumerable<CourseSearchForDashboardClientResponse>>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<CourseSearchForDashboardClientQueriesHandler> _logger = logger;

    public async Task<Result<IEnumerable<CourseSearchForDashboardClientResponse>>> Handle(CourseSearchForDashboardClientQueries request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.SearchAsync<CourseSearch>(c => c
                .Indices(ConstantIndexElastic.CourseIndex)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.IsPublished)
                        .Value(true)
                    )
                )
                .Sort(s => s
                    .Field(f => f
                        .Field(ff => ff.OrderIndex)
                        .Order(SortOrder.Asc)
                    )
                )
            , cancellationToken);
            var result = response.Documents.Select(doc => new CourseSearchForDashboardClientResponse(
                Id: doc.CourseId,
                Title: doc.Title,
                Description: doc.Description,
                HskLevel: doc.HskLevel,
                Slug: doc.Slug,
                OrderIndex: doc.OrderIndex,
                TotalTopics: doc.TotalTopicsPublished,
                TotalStudentsEnrolled: doc.TotalStudentsEnrolled
            )).ToList();
            return Result<IEnumerable<CourseSearchForDashboardClientResponse>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for courses for dashboard. Details: {Message}", ex.Message);
            return Result<IEnumerable<CourseSearchForDashboardClientResponse>>.FailureResult(
                "An error occurred while loading courses. Please try again later.",
                errorCode: (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}

