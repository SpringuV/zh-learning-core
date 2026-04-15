namespace Search.Infrastructure.Queries.Lesson.Search;

// Vì CourseTopicsOverviewAdminQueries có cùng tham số truyền vào giống với 
// TopicSearchAdminQueries nên có thể tận dụng lại handler của TopicSearchAdminQueries 
// để thực hiện tìm kiếm các topic, sau đó chỉ cần thêm bước truy vấn metadata của course 
// rồi gộp kết quả trả về là được
public sealed record CourseTopicsOverviewAdminQueries(
    Guid CourseId,
    string? Title = null,
    bool? IsPublished = null,
    string? TopicType = null,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null,
    int Take = 30,
    string? SearchAfterValues = null,
    TopicSortBy SortBy = TopicSortBy.CreatedAt,
    bool OrderByDescending = true
) : IRequest<TopicSearchWithCourseMetadataResponse>;

public sealed class CourseTopicsOverviewAdminQueriesHandler(
    IMediator mediator,
    ElasticsearchClient client,
    ILogger<CourseTopicsOverviewAdminQueriesHandler> logger)
    : IRequestHandler<CourseTopicsOverviewAdminQueries, TopicSearchWithCourseMetadataResponse>
{
    private readonly IMediator _mediator = mediator;
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<CourseTopicsOverviewAdminQueriesHandler> _logger = logger;

    public async Task<TopicSearchWithCourseMetadataResponse> Handle(CourseTopicsOverviewAdminQueries request, CancellationToken cancellationToken)
    {
        // Vì CourseTopicsOverviewAdminQueries và TopicSearchAdminQueries có cùng tham số 
        // truyền vào nên có thể tận dụng lại handler của TopicSearchAdminQueries để thực 
        // hiện tìm kiếm các topic, sau đó chỉ cần thêm bước truy vấn metadata của course
        //  rồi gộp kết quả trả về là được
        var topicsResult = await _mediator.Send(new TopicSearchAdminQueries(
            CourseId: request.CourseId,
            Title: request.Title,
            IsPublished: request.IsPublished,
            TopicType: request.TopicType,
            StartCreatedAt: request.StartCreatedAt,
            EndCreatedAt: request.EndCreatedAt,
            Take: request.Take,
            SearchAfterValues: request.SearchAfterValues,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending
        ), cancellationToken);
        // get metadata của course để trả về cùng với kết quả tìm kiếm topic, vì trong UI sẽ có một số thông tin của course cần hiển thị ở trang quản lý topic như title, hsk level, slug, ... nên việc lấy metadata của course sẽ giúp giảm thiểu số lượng request cần thiết từ client thay vì phải gọi thêm một API riêng để lấy thông tin course sau khi đã có kết quả tìm kiếm topic rồi mới gọi tiếp API lấy thông tin course nữa
        var courseMetadata = await LoadCourseMetadataAsync(request.CourseId, cancellationToken);

        return new TopicSearchWithCourseMetadataResponse(
            Course: courseMetadata,
            Total: topicsResult.Total,
            Items: topicsResult.Items,
            HasNextPage: topicsResult.HasNextPage,
            NextCursor: topicsResult.NextCursor
        );
    }

    private async Task<CourseMetadataForTopicAdminResponse?> LoadCourseMetadataAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.CourseIndex, cancellationToken);
        if (!indexExistsResponse.Exists)
        {
            _logger.LogInformation("Index {IndexName} does not exist. Course metadata cannot be loaded.", ConstantIndexElastic.CourseIndex);
            return null;
        }

        var response = await _client.SearchAsync<CourseSearch>(s => s
            .Indices(ConstantIndexElastic.CourseIndex)
            .Size(1)
            .Query(q => q.Term(t => t
                .Field(f => f.CourseId.Suffix("keyword"))
                .Value(courseId.ToString("D")))),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogWarning("Failed to load course metadata for CourseId {CourseId}. DebugInfo: {DebugInfo}",
                courseId,
                response.DebugInformation);
            return null;
        }

        var course = response.Documents.FirstOrDefault();
        if (course is null)
        {
            return null;
        }

        return new CourseMetadataForTopicAdminResponse(
            Id: course.CourseId,
            Title: course.Title,
            HskLevel: course.HskLevel,
            Slug: course.Slug,
            IsPublished: course.IsPublished,
            TotalTopics: course.TotalTopics,
            TotalStudentsEnrolled: course.TotalStudentsEnrolled
        );
    }
}
