namespace Search.Infrastructure.Queries.Lesson.Client;

public record TopicSearchForDashboardClientQueries(string Slug, Guid UserId)
    : IRequest<Result<IEnumerable<TopicSearchForDashboardClientResponse>>>,
    ICacheScopeRequest,
    ICacheableRequest<Result<IEnumerable<TopicSearchForDashboardClientResponse>>>
{
    public string CacheKey => $"TopicDashClient:Slug:{Slug}:User:{UserId}";

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
                    (int)ErrorCode.INTERNAL_ERROR
                );
            }

            var course = responseCourse.Documents.FirstOrDefault();
            if (course is null)
            {
                _logger.LogInformation("No course found for slug {Slug}. Returning empty topic dashboard result.", request.Slug);
                return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.SuccessResult([]);
            }

            var queries = _client.SearchAsync<TopicSearch>(s => s
                .Indices(ConstantIndexElastic.TopicIndex)
                .Query(q => q
                    .Bool(b => b
                        .Filter(
                            f => f.Term(t => t
                                .Field(ff => ff.CourseId.Suffix("keyword"))
                                .Value(course.CourseId.ToString())
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

            // step 2: map kết quả trả về, với mỗi topic thì cần load thêm status của topic đó dựa vào topic exercise session (nếu có) để hiển thị trên dashboard (ví dụ nếu đang học dở thì hiển thị in-progress, đã học xong thì hiển thị completed, chưa học thì hiển thị not started)
            var topics = response.Documents.ToList();
            if (topics.Count == 0)
            {
                return Result<IEnumerable<TopicSearchForDashboardClientResponse>>.SuccessResult([]);
            }

            // ưu tiên status từ session index (nguồn state gần nhất cho flow học tiếp), lấy bằng userId và topicId, nếu có session nào đang in-progress hoặc completed thì ưu tiên hiển thị trạng thái đó trên dashboard, nếu không có session nào hoặc tất cả session đều là abandoned thì hiển thị not started
            // dictionary này sẽ chứa session mới nhất (theo updatedAt) cho mỗi topicId, nếu có nhiều session cho cùng 1 topicId thì sẽ lấy session có updatedAt mới nhất để đảm bảo
            // <Guid topicId, TopicExerciseSessionSearch session mới nhất cho topic đó>
            var topicSessionByTopicId = new Dictionary<Guid, TopicExerciseSessionSearch>();
            var sessionIndexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicExerciseSessionIndex, cancellationToken);
            if (sessionIndexExistsResponse.Exists)
            {
                var sessionResponse = await _client.SearchAsync<TopicExerciseSessionSearch>(s => s
                    .Indices(ConstantIndexElastic.TopicExerciseSessionIndex)
                    .Size(1000)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.UserId.Suffix("keyword"))
                            .Value(request.UserId.ToString())
                        )
                    ),
                    cancellationToken);

                if (!sessionResponse.IsValidResponse)
                {
                    _logger.LogWarning("Invalid Elasticsearch response when loading topic exercise sessions for user {UserId}. Defaulting status to Abandoned. DebugInfo: {DebugInfo}",
                        request.UserId,
                        sessionResponse.DebugInformation);
                }
                else
                {
                    // nhóm các session theo topicId, với mỗi topicId chỉ lấy session có updatedAt mới nhất để đảm bảo lấy được trạng thái gần nhất của topic đó
                    topicSessionByTopicId = sessionResponse.Documents
                        .GroupBy(x => x.TopicId)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.UpdatedAt).First());
                }
            }

            // hàm phụ để xác định status của topic trên dashboard dựa vào session (nếu có)
            static StatusTopicForDashboardClient ResolveStatus(
                Guid topicId,
                IReadOnlyDictionary<Guid, TopicExerciseSessionSearch> sessionByTopicId)
            {
                // nếu có session nào đang in-progress hoặc completed thì ưu tiên hiển thị trạng thái đó trên dashboard, nếu không có session nào hoặc tất cả session đều là abandoned thì hiển thị not started
                // cái try get value này sẽ so sánh topicId của topic đang xét với các topicId trong sessionByTopicId, nếu tìm thấy thì lấy session tương ứng để xác định trạng thái hiển thị trên dashboard, nếu không tìm thấy thì mặc định là not started
                if (sessionByTopicId.TryGetValue(topicId, out var session))
                {
                    return session.Status switch
                    {
                        StatusTopicForDashboardClient.InProgress => StatusTopicForDashboardClient.InProgress,
                        StatusTopicForDashboardClient.Completed => StatusTopicForDashboardClient.Completed,
                        StatusTopicForDashboardClient.Abandoned => StatusTopicForDashboardClient.Abandoned,
                        StatusTopicForDashboardClient.NotStarted => StatusTopicForDashboardClient.NotStarted,
                        _ => StatusTopicForDashboardClient.NotStarted
                    };
                }

                return StatusTopicForDashboardClient.NotStarted;
            }

            var result = topics.Select(doc => new TopicSearchForDashboardClientResponse(
                Id: doc.TopicId,
                Title: doc.Title,
                Slug: doc.Slug,
                OrderIndex: doc.OrderIndex,
                TopicType: doc.TopicType.ToString(),
                ExamYear: doc.ExamYear,
                ExamCode: doc.ExamCode,
                EstimatedTimeMinutes: doc.EstimatedTimeMinutes,
                Description: doc.Description,
                Status: ResolveStatus(doc.TopicId, topicSessionByTopicId), // xác định status của topic trên dashboard dựa vào session (nếu có)
                TotalExercises: doc.TotalExercises
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