namespace Search.Infrastructure.Queries.Lesson.Update.Exercise;

#region UpdateSearch Command
public sealed record ExerciseUnPublishedSearchCommand(
    Guid ExerciseId,
    DateTime UnpublishedAt
) : IRequest<Unit>;
public sealed record ExercisePublishedSearchCommand(
    Guid ExerciseId,
    DateTime PublishedAt
) : IRequest<Unit>;
public sealed record ExerciseReOrderSearchCommand(
    Guid TopicId,
    IReadOnlyList<Guid> OrderedExerciseIds,
    DateTime UpdatedAt
) : IRequest<Unit>;
public sealed record ExercisePatchSearchCommand(
    Guid ExerciseId,
    object PatchDocument
) : IRequest<Unit>;
#endregion

#region UnPublish Handlers
public class ExerciseUnPublishedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<ExerciseUnPublishedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(ExerciseUnPublishedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<ExerciseSearch, object>(
            ConstantIndexElastic.ExerciseIndex,
            request.ExerciseId,
            u => u.Doc(new
            {
                IsPublished = false,
                request.UnpublishedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update exercise with id {request.ExerciseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region Publish Handlers
public class ExercisePublishedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<ExercisePublishedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(ExercisePublishedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<ExerciseSearch, object>(
            ConstantIndexElastic.ExerciseIndex,
            request.ExerciseId,
            u => u.Doc(new
            {
                IsPublished = true,
                request.PublishedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update exercise with id {request.ExerciseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region ReOrder Handlers
public class ExerciseReOrderSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<ExerciseReOrderSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(ExerciseReOrderSearchCommand request, CancellationToken cancellationToken)
    {
        // hiện tại chưa cần xử lý bằng cái IdTopic vì khi re-order thì chắc chắn
        // các exercise đó đã thuộc về topic rồi, nên chỉ cần update lại orderIndex 
        // và updatedAt là được
        var buldRequest = new BulkRequest();
        var operation = new List<IBulkOperation>();
        for(int i = 0; i < request.OrderedExerciseIds.Count; i++)
        {
            operation.Add(new BulkUpdateOperation<ExerciseSearch, object>(request.OrderedExerciseIds[i])
            {
                Index = ConstantIndexElastic.ExerciseIndex,
                Doc = new
                {
                    request.UpdatedAt,
                    OrderIndex = i + 1
                }
            });
        }
        // Gán danh sách các operation vào BulkRequest để thực hiện một lần gọi duy nhất đến Elasticsearch, tối ưu hiệu suất và giảm thiểu số lượng request cần thiết
        buldRequest.Operations = operation; 
        var response = await _elasticClient.BulkAsync(buldRequest, cancellationToken);

        if (!response.Errors)
        {
            return Unit.Value;
        }

        var failedItems = response.ItemsWithErrors
            .Select(x => x.Error?.Reason)
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .ToList();
        var reasonText = failedItems.Count > 0
            ? string.Join("; ", failedItems)
            : (response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation);

        throw new InvalidOperationException($"Failed to reorder exercises in Elasticsearch. Errors: {reasonText}");
    }
}
#endregion

#region Patch Handler
public class ExercisePatchSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<ExercisePatchSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(ExercisePatchSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<ExerciseSearch, object>(
            ConstantIndexElastic.ExerciseIndex,
            request.ExerciseId,
            u => u.Doc(request.PatchDocument).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to patch exercise with id {request.ExerciseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion