namespace Search.Infrastructure.Queries.Lesson.Delete;

public sealed record CourseDeletedSearchCommand(
    Guid CourseId
) : IRequest<Unit>;

public sealed class CourseDeletedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseDeletedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseDeletedSearchCommand request, CancellationToken cancellationToken)
    {
        // Sử dụng phương thức DeleteAsync để xóa tài liệu khỏi Elasticsearch
        // cú pháp: client.DeleteAsync<TDocument>(index, id, selector, cancellationToken)
        var response = await _elasticClient.DeleteAsync<CourseSearch>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            c => c.Id(request.CourseId), // đảm bảo rằng chúng ta đang xóa tài liệu với ID chính xác
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to delete course with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}