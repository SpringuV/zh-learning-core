namespace Search.Infrastructure.Queries.Lesson.Search.Detail;

public sealed record TopicSearchDetailAdminQueries(Guid TopicId)
    : IRequest<TopicSearchDetailResponse>, ICacheableRequest<TopicSearchDetailResponse>, ICacheScopeRequest
{
    public string CacheKey => $"topic-detail-s-adm:{TopicId}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);

// Đặt cache scope để phân biệt với các loại search khác, đảm bảo cache được tổ chức tốt và dễ dàng invalid khi cần thiết
    public string CacheScope => SearchCacheScopes.TopicAdminSearch;
}

public class TopicSearchDetailAdminQueriesHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicSearchDetailAdminQueries, TopicSearchDetailResponse>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<TopicSearchDetailResponse> Handle(TopicSearchDetailAdminQueries request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.GetAsync<TopicSearch>(request.TopicId, idx => idx.Index(ConstantIndexElastic.TopicIndex), cancellationToken);
        if (!response.Found || response.Source == null)
        {
            throw new KeyNotFoundException($"Topic with ID {request.TopicId} not found in search index.");
        }
        var topic = response.Source;
        return new TopicSearchDetailResponse(
            Id: topic.TopicId,
            Title: topic.Title,
            Slug: topic.Slug,
            OrderIndex: topic.OrderIndex,
            TopicType: topic.TopicType.ToString(),
            ExamYear: topic.ExamYear,
            ExamCode: topic.ExamCode,
            EstimatedTimeMinutes: topic.EstimatedTimeMinutes,
            Description: topic.Description,
            IsPublished: topic.IsPublished,
            TotalExercises: topic.TotalExercises,
            CreatedAt: topic.CreatedAt,
            UpdatedAt: topic.UpdatedAt
        );
    }
}