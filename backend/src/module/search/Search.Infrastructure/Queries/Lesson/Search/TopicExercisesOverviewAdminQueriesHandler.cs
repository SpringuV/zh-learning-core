namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record TopicExercisesOverviewAdminQueries(
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
    // keyset pagination sẽ dùng SearchAfter, sẽ lấy những document có CreatedAt nhỏ hơn timestamp của document cuối cùng trong page trước, để tránh việc skip nhiều document khi trang có nhiều kết quả
    string? SearchAfterValues = null, // dùng để phân trang, timestamp của document cuối cùng trong page trước, sẽ lấy những document có CreatedAt nhỏ hơn timestamp này
    ExerciseSortBy SortBy = ExerciseSortBy.CreatedAt,
    bool OrderByDescending = true
) : IRequest<ExerciseSearchWithTopicMetadataResponse>,
    ICacheableRequest<ExerciseSearchWithTopicMetadataResponse>,
    ICacheScopeRequest
{
    public string CacheKey => $"TopicExercisesOverviewAdmin:{TopicId}:{Question}:{IsPublished}:{SkillType}:{ExerciseType}:{Difficulty}:{Context}:{StartCreatedAt}:{EndCreatedAt}:{Take}:{SearchAfterValues}:{SortBy}:{OrderByDescending}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(1); // cache kết quả trong 5 phút

    public string CacheScope => SearchCacheScopes.ExerciseAdminSearch;
}

public sealed class TopicExercisesOverviewAdminQueriesHandler(
    IMediator mediator,
    ElasticsearchClient client,
    ILogger<TopicExercisesOverviewAdminQueriesHandler> logger)
    : IRequestHandler<TopicExercisesOverviewAdminQueries, ExerciseSearchWithTopicMetadataResponse>
{
    private readonly IMediator _mediator = mediator;
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<TopicExercisesOverviewAdminQueriesHandler> _logger = logger;

    public async Task<ExerciseSearchWithTopicMetadataResponse> Handle(TopicExercisesOverviewAdminQueries request, CancellationToken cancellationToken)
    {
        var queries = new ExerciseSearchAdminQueries(
            TopicId: request.TopicId,
            Question: request.Question,
            SkillType: request.SkillType,
            ExerciseType: request.ExerciseType,
            Difficulty: request.Difficulty,
            Context: request.Context,
            StartCreatedAt: request.StartCreatedAt,
            EndCreatedAt: request.EndCreatedAt,
            Take: request.Take,
            SearchAfterValues: request.SearchAfterValues,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending
        );
        // Vì TopicExercisesOverviewAdminQueries có cùng tham số truyền vào giống với 
        // ExerciseSearchAdminQueries nên có thể tận dụng lại handler của ExerciseSearchAdminQueries
        //  để thực hiện tìm kiếm các exercise, sau đó chỉ cần thêm bước truy vấn metadata
        //  của topic rồi gộp kết quả trả về là được
        var searchResult = await _mediator.Send(queries, cancellationToken);
        var topicMetadata = await LoadTopicMetadataAsync(request.TopicId, cancellationToken);
        return new ExerciseSearchWithTopicMetadataResponse(
            Total: searchResult.Total,
            Items: searchResult.Items,
            HasNextPage: searchResult.HasNextPage,
            NextCursor: searchResult.NextCursor,
            ParentMetadata: topicMetadata
        );
    }

    private async Task<TopicMetadataForExerciseAdminResponse?> LoadTopicMetadataAsync(Guid topicId, CancellationToken cancellationToken)
    {
        var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicIndex, cancellationToken);
        if (!indexExistsResponse.Exists)
        {
            _logger.LogWarning("Topic index does not exist in Elasticsearch.");
            return null;
        }
        var response = await _client.SearchAsync<TopicSearch>(s => s
            .Indices(ConstantIndexElastic.TopicIndex)
            .Size(1)
            .Query(q => q.Term(t => t.Field(f => f.TopicId.Suffix("keyword")).Value(topicId.ToString("D")))), cancellationToken
            );
        if(!response.IsValidResponse)
        {
             _logger.LogWarning("Failed to load topic metadata for TopicId {TopicId}. DebugInfo: {DebugInfo}",
                topicId,
                response.DebugInformation);
            return null;
        }

        var topic = response.Documents.FirstOrDefault();
        if (topic is null)
        {
            return null;
        }

        return new TopicMetadataForExerciseAdminResponse(
            Id: topic.TopicId,
            Title: topic.Title,
            EstimatedTimeMinutes: topic.EstimatedTimeMinutes,
            IsPublished: topic.IsPublished,
            TopicType: topic.TopicType.ToString(),
            ExamYear: topic.ExamYear,
            ExamCode: topic.ExamCode,
            Slug: topic.Slug,
            TotalExercises: topic.TotalExercises
        );
    }
}