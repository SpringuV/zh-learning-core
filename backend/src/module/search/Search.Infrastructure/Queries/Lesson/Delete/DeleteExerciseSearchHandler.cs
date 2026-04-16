namespace Search.Infrastructure.Queries.Lesson.Delete;

public sealed record ExerciseDeletedSearchCommand(
    Guid ExerciseId
) : IRequest<Unit>;

public sealed class ExerciseDeletedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<ExerciseDeletedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(ExerciseDeletedSearchCommand request, CancellationToken cancellationToken)
    {
        // Sử dụng phương thức DeleteAsync để xóa tài liệu khỏi Elasticsearch
        // cú pháp: client.DeleteAsync<TDocument>(index, id, selector, cancellationToken)
        var response = await _elasticClient.DeleteAsync<ExerciseSearch>(
            ConstantIndexElastic.ExerciseIndex,
            request.ExerciseId,
            e => e.Id(request.ExerciseId), // đảm bảo rằng chúng ta đang xóa tài liệu với ID chính xác
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to delete exercise with id {request.ExerciseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}