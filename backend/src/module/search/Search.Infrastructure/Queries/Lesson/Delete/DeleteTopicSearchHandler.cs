namespace Search.Infrastructure.Queries.Lesson.Delete;

public sealed record TopicDeletedSearchCommand(
    Guid TopicId
) : IRequest<Unit>;

public sealed class TopicDeletedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicDeletedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicDeletedSearchCommand request, CancellationToken cancellationToken)
    {
        // Sử dụng phương thức DeleteAsync để xóa tài liệu khỏi Elasticsearch
        // cú pháp: client.DeleteAsync<TDocument>(index, id, selector, cancellationToken)
        var response = await _elasticClient.DeleteAsync<TopicSearch>(
            ConstantIndexElastic.TopicIndex,
            request.TopicId,
            t => t.Id(request.TopicId), // đảm bảo rằng chúng ta đang xóa tài liệu với ID chính xác
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to delete topic with id {request.TopicId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}