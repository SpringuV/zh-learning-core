namespace Search.Infrastructure.Queries.Lesson.Update.Topic;

#region UpdateSearch Command
public sealed record TopicUnPublishedSearchCommand(
    Guid TopicId,
    DateTime UnpublishedAt
) : IRequest<Unit>;

public sealed record TopicPublishedSearchCommand(
    Guid TopicId,
    DateTime PublishedAt
) : IRequest<Unit>;

public sealed record TopicReOrderSearchCommand(
    Guid CourseId,
    IReadOnlyList<Guid> OrderedTopicIds,
    DateTime UpdatedAt
) : IRequest<Unit>;

public sealed record TopicTotalExercisesUpdatedSearchCommand(
    Guid TopicId,
    long TotalExercises,
    DateTime UpdatedAt
): IRequest<Unit>;

public sealed record TopicPatchSearchCommand(
    Guid TopicId,
    object PatchDocument
) : IRequest<Unit>;

public sealed record TopicTotalExercisePublishedUpdatedSearchQueries(
    Guid TopicId,
    int TotalExercisesPublished,
    DateTime UpdatedAt
): IRequest<Unit>;
#endregion

#region UnPublish Handlers
public class TopicUnPublishedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicUnPublishedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicUnPublishedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<TopicSearch, object>(
            ConstantIndexElastic.TopicIndex,
            request.TopicId,
            u => u.Doc(new
            {
                IsPublished = false,
                request.UnpublishedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update topic with id {request.TopicId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion


#region Publish Handlers
public class TopicPublishedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicPublishedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicPublishedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<TopicSearch, object>(
            ConstantIndexElastic.TopicIndex,
            request.TopicId,
            u => u.Doc(new
            {
                IsPublished = true,
                request.PublishedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to publish topic with id {request.TopicId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region ReOrder Handlers
public class TopicReOrderSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicReOrderSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicReOrderSearchCommand request, CancellationToken cancellationToken)
    {
        var buldRequest = new BulkRequest();
        var operations = new List<IBulkOperation>();
        for (int i = 0; i < request.OrderedTopicIds.Count; i++)
        { 
            operations.Add(new BulkUpdateOperation<TopicSearch, object>(request.OrderedTopicIds[i])
            {
                Index = ConstantIndexElastic.TopicIndex,
                Doc = new { OrderIndex = i + 1, request.UpdatedAt }
            });
        }
        buldRequest.Operations = operations;
        var response = await _elasticClient.BulkAsync(buldRequest, cancellationToken);
        
        if(!response.Errors)
        {
            return Unit.Value;
        }
        var failedItems = response.ItemsWithErrors
            .Select(x => x.Error?.Reason).Where(reason => !string.IsNullOrWhiteSpace(reason)).ToList();
        var responseText = failedItems.Count > 0 
            ? string.Join("; ", failedItems) 
            : "Unknown error";
        throw new InvalidOperationException($"Failed to reorder topics in Elasticsearch. Reason: {responseText}");
    }
}
#endregion

#region TotalExercises Handler
public class TopicTotalExercisesUpdatedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicTotalExercisesUpdatedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicTotalExercisesUpdatedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<TopicSearch, object>(
            ConstantIndexElastic.TopicIndex,
            request.TopicId,
            u => u.Doc(new
            {
                request.TotalExercises,
                request.UpdatedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update total exercises for topic with id {request.TopicId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region Patch Handler
public class TopicPatchSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicPatchSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicPatchSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<TopicSearch, object>(
            ConstantIndexElastic.TopicIndex,
            request.TopicId,
            u => u.Doc(request.PatchDocument).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to patch topic with id {request.TopicId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region TotalExercisePublished
public class TopicTotalExercisePublishedUpdatedSearchQueriesHandler(ElasticsearchClient elasticClient) : IRequestHandler<TopicTotalExercisePublishedUpdatedSearchQueries, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(TopicTotalExercisePublishedUpdatedSearchQueries request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<TopicSearch, object>(
            ConstantIndexElastic.TopicIndex,
            request.TopicId,
            u => u.Doc(new
            {
                request.TotalExercisesPublished,
                request.UpdatedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update total exercises published for topic with id {request.TopicId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion