namespace Search.Infrastructure.Queries.Lesson.Indexs;

public record CourseIndexCommand(
    Guid Id,
    string Title,
    string Description,
    int OrderIndex,
    int HskLevel,
    string Slug,
    long TotalTopics,
    long TotalStudentsEnrolled,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt) : IRequest<CourseIndexResponse>;

public class CourseIndexCommandHandler(ElasticsearchClient client, ILogger<CourseIndexCommandHandler> logger) : IRequestHandler<CourseIndexCommand, CourseIndexResponse>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<CourseIndexCommandHandler> _logger = logger;

    public async Task<CourseIndexResponse> Handle(CourseIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureCourseIndexExistsAsync(cancellationToken);
            var courseDocument = new CourseSearch
            {
                CourseId = request.Id,
                Title = request.Title,
                Description = request.Description,
                OrderIndex = request.OrderIndex,
                HskLevel = request.HskLevel,
                Slug = request.Slug,
                TotalTopics = request.TotalTopics,
                TotalStudentsEnrolled = request.TotalStudentsEnrolled,
                IsPublished = request.IsPublished,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };

            var response = await _client.IndexAsync(courseDocument, i => i.Index(ConstantIndexElastic.CourseIndex).Id(courseDocument.CourseId), cancellationToken);
            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to index course {request.Id}: {response.DebugInformation}");
            }

            _logger.LogInformation("Indexed course {CourseId} with title: {Title}", request.Id, request.Title);

            return new CourseIndexResponse(
                CourseId: courseDocument.CourseId,
                CreatedAt: courseDocument.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index course {CourseId}", request.Id);
            throw;
        }
    }
    private async Task EnsureCourseIndexExistsAsync(CancellationToken cancellationToken)
    {
        var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.CourseIndex, cancellationToken);
        if (indexExistsResponse.Exists)
        {
            return;
        }
        
        // Create the index with a mapping
        var createResponse = await _client.Indices.CreateAsync(ConstantIndexElastic.CourseIndex, c => c
            .Mappings<CourseSearch>(m => m
                .Properties(p => p
                    .Keyword(c => c.CourseId, k => k.Fields(f => f.Keyword("keyword")))
                    .Text(c => c.Title, t => t.Fields(f => f.Keyword("keyword")))
                    .Text(c => c.Description, t => t.Fields(f => f.Keyword("keyword")))
                    .IntegerNumber(c => c.OrderIndex)
                    .IntegerNumber(c => c.HskLevel)
                    .Text(c => c.Slug, t => t.Fields(f => f.Keyword("keyword")))
                    .LongNumber(c => c.TotalTopics)
                    .LongNumber(c => c.TotalStudentsEnrolled)
                    .Boolean(c => c.IsPublished)
                    .Date(c => c.CreatedAt)
                    .Date(c => c.UpdatedAt)
                )
            ), cancellationToken);
        if (!createResponse.IsValidResponse)
        {
            throw new Exception($"Failed to create index {ConstantIndexElastic.CourseIndex}: {createResponse.DebugInformation}");
        }
    }
}