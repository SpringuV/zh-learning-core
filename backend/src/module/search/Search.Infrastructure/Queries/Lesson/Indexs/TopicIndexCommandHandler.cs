using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Indexs;

public sealed record TopicIndexCommand(
    Guid CourseId,
    Guid TopicId,
    string Title,
    string Description,
    string Slug,
    int TotalExercisesPublished,
    string TopicType,
    long EstimatedTimeMinutes,
    int ExamYear,
    string ExamCode,
    int OrderIndex,
    bool IsPublished,
    long TotalExercises,
    DateTime CreatedAt,
    DateTime UpdatedAt
): IRequest<TopicIndexResponse>;

public class TopicIndexCommandHandler(ElasticsearchClient client, ILogger<TopicIndexCommandHandler> logger) : IRequestHandler<TopicIndexCommand, TopicIndexResponse>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<TopicIndexCommandHandler> _logger = logger;

    public async Task<TopicIndexResponse> Handle(TopicIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate topic type trước khi indexing để tránh lỗi không mong muốn khi lưu vào Elasticsearch
            if (!Enum.TryParse<TopicType>(request.TopicType, true, out var parsedTopicType) ||
                !Enum.IsDefined(parsedTopicType))
            {
                throw new ArgumentException(
                    $"TopicType '{request.TopicType}' không hợp lệ. Giá trị hỗ trợ: {string.Join(", ", Enum.GetNames<TopicType>())}.",
                    nameof(request.TopicType));
            }

            await EnsureTopicIndexExistsAsync(cancellationToken);
            var topicDocument = new TopicSearch
            {
                CourseId = request.CourseId,
                TopicId = request.TopicId,
                Title = request.Title,
                Description = request.Description,
                Slug = request.Slug,
                TopicType = parsedTopicType,
                EstimatedTimeMinutes = request.EstimatedTimeMinutes,
                ExamYear = request.ExamYear,
                ExamCode = request.ExamCode,
                OrderIndex = request.OrderIndex,
                IsPublished = request.IsPublished,
                TotalExercises = request.TotalExercises,
                TotalExercisesPublished = request.TotalExercisesPublished,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };

            var response = await _client.IndexAsync(topicDocument, i => i
                    .Index(ConstantIndexElastic.TopicIndex)
                    .Id(topicDocument.TopicId), cancellationToken);
            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to index topic {request.TopicId}: {response.DebugInformation}");
            }

            _logger.LogInformation("Indexed topic {TopicId} with title: {Title}", request.TopicId, request.Title);

            return new TopicIndexResponse(
                TopicId: topicDocument.TopicId,
                CreatedAt: topicDocument.CreatedAt
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid topic type when indexing topic {TopicId}: {TopicType}", request.TopicId, request.TopicType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index topic {TopicId}", request.TopicId);
            throw;
        }
    }

    private async Task EnsureTopicIndexExistsAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicIndex, cancellationToken);
        if (!existsResponse.Exists)
        {
            var createIndexResponse = await _client.Indices.CreateAsync(ConstantIndexElastic.TopicIndex, c => c
                .Mappings<TopicSearch>(m => m
                    .Properties(p => p
                        // keyword để filter chính xác theo id, tránh lỗi do analyzer khi index (GUID có dấu gạch ngang sẽ bị cắt thành nhiều token)
                        .Keyword(tp => tp.TopicId, k => k.Fields(f => f.Keyword("keyword")))
                        .Keyword(tp => tp.CourseId, k => k.Fields(f => f.Keyword("keyword")))
                        .Text(tp => tp.Title, t => t.Fields(f => f.Keyword("keyword")))
                        .Text(tp => tp.Description, t => t.Fields(f => f.Keyword("keyword")))
                        .Text(tp => tp.Slug, t => t.Fields(f => f.Keyword("keyword")))
                        .Keyword(tp => tp.TopicType) // Lưu topic type dưới dạng keyword để dễ dàng filter và tránh lỗi khi parse enum
                        .IntegerNumber(tp => tp.EstimatedTimeMinutes)
                        .IntegerNumber(tp => tp.TotalExercisesPublished)
                        .IntegerNumber(tp => tp.TotalExercises)
                        .IntegerNumber(tp => tp.ExamYear)
                        .Text(tp => tp.ExamCode, t => t.Fields(f => f.Keyword("keyword")))
                        .IntegerNumber(tp => tp.OrderIndex)
                        .Boolean(tp => tp.IsPublished)
                        .Date(tp => tp.CreatedAt)
                        .Date(tp => tp.UpdatedAt)
                    )), cancellationToken);

            if (!createIndexResponse.IsValidResponse)
            {
                throw new Exception($"Failed to create Elasticsearch index for topics: {createIndexResponse.DebugInformation}");
            }
        }
    }
}