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

        // Delete trong Elasticsearch cần idempotent: nếu tài liệu đã bị xóa trước đó (result=not_found)
        // thì xem như thành công để tránh outbox retry vô hạn.
        var isNotFound = response.DebugInformation?.Contains("\"result\":\"not_found\"", StringComparison.OrdinalIgnoreCase) == true
            || response.DebugInformation?.Contains("successful (404)", StringComparison.OrdinalIgnoreCase) == true;

        if (!response.IsValidResponse && !isNotFound)
        {
            throw new InvalidOperationException($"Failed to delete course with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}