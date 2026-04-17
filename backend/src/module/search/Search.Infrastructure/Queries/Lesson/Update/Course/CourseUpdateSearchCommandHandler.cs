namespace Search.Infrastructure.Queries.Lesson.Update.Course;

#region Command

public sealed record CourseUnPublishedSearchCommand(
    Guid CourseId,
    DateTime UnpublishedAt
) : IRequest<Unit>;
public sealed record CoursePublishedSearchCommand(
    Guid CourseId,
    DateTime PublishedAt
) : IRequest<Unit>;
public sealed record CourseTitleUpdatedSearchCommand(
    Guid CourseId,
    string NewTitle,
    string NewSlug,
    DateTime UpdatedAt
) : IRequest<Unit>;
public sealed record CourseDescriptionUpdatedSearchCommand(
    Guid CourseId,
    string NewDescription,
    DateTime UpdatedAt
) : IRequest<Unit>;
public sealed record CourseHskLevelUpdatedSearchCommand(
    Guid CourseId,
    int NewHskLevel,
    DateTime UpdatedAt
) : IRequest<Unit>;
public sealed record CourseReOrderedSearchCommand(
    List<Guid> OrderedCourseIds,
    DateTime UpdatedAt
) : IRequest<Unit>;

public sealed record CourseTotalTopicsUpdatedSearchCommand(
    Guid CourseId,
    long TotalTopics,
    DateTime UpdatedAt
) : IRequest<Unit>;
#endregion

#region TotalTopicsUpdate Handlers
public class CourseTotalTopicsUpdatedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseTotalTopicsUpdatedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseTotalTopicsUpdatedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<CourseSearch, object>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            u => u.Doc(new
            {
                request.TotalTopics, // vì TotalTopics là trường có tên giống với property trong CourseSearch nên có thể sử dụng cú pháp rút gọn mà không cần phải viết TotalTopics = request.TotalTopics
                request.UpdatedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update total topics for course with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region Publish Handlers
public class CoursePublishedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CoursePublishedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CoursePublishedSearchCommand request, CancellationToken cancellationToken)
    {   
        var response = await _elasticClient.UpdateAsync<CourseSearch, object>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            u => u.Doc(new
            {
                UpdatedAt = request.PublishedAt,
                IsPublished = true
            }).RetryOnConflict(3),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to publish course with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region UnPublish Handlers
public class CourseUnPublishedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseUnPublishedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseUnPublishedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<CourseSearch, object>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            u => u.Doc(new
            {
                UpdatedAt = request.UnpublishedAt,
                IsPublished = false
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to unpublish course with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region TitleUpdate Handlers
public class CourseTitleUpdatedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseTitleUpdatedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseTitleUpdatedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<CourseSearch, object>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            u => u.Doc(new
            {
                Title = request.NewTitle,
                Slug = request.NewSlug,
                UpdatedAt = request.UpdatedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update course title with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region DescriptionUpdate Handlers

public class CourseDescriptionUpdatedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseDescriptionUpdatedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseDescriptionUpdatedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<CourseSearch, object>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            u => u.Doc(new
            {
                Description = request.NewDescription,
                UpdatedAt = request.UpdatedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update course description with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region HskLevelUpdate Handlers

public class CourseHskLevelUpdatedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseHskLevelUpdatedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseHskLevelUpdatedSearchCommand request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.UpdateAsync<CourseSearch, object>(
            ConstantIndexElastic.CourseIndex,
            request.CourseId,
            u => u.Doc(new
            {
                HskLevel = request.NewHskLevel,
                UpdatedAt = request.UpdatedAt
            }).RetryOnConflict(3),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to update course HSK level with id {request.CourseId} in Elasticsearch. Reason: {(response.TryGetOriginalException(out var ex) ? ex!.Message : response.DebugInformation)}");
        }

        return Unit.Value;
    }
}
#endregion

#region ReOrder Handlers

public class CourseReOrderedSearchCommandHandler(ElasticsearchClient elasticClient) : IRequestHandler<CourseReOrderedSearchCommand, Unit>
{
    private readonly ElasticsearchClient _elasticClient = elasticClient;

    public async Task<Unit> Handle(CourseReOrderedSearchCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderedCourseIds.Count == 0)
        {
            return Unit.Value;
        }

        var distinctCourseIdCount = request.OrderedCourseIds.Distinct().Count();
        if (distinctCourseIdCount != request.OrderedCourseIds.Count)
        {
            throw new ArgumentException("OrderedCourseIds contains duplicate values.", nameof(request.OrderedCourseIds));
        }

        var ids = request.OrderedCourseIds
            .Select(courseId => (Id)courseId.ToString("D"))
            .ToArray();

        var multiGetResponse = await _elasticClient.MultiGetAsync<CourseSearch>(m => m
                .Index(ConstantIndexElastic.CourseIndex)
                .Ids(new Ids(ids)),
            cancellationToken);

        if (!multiGetResponse.IsValidResponse)
        {
            var errorMessage = multiGetResponse.TryGetOriginalException(out var originalException)
                ? originalException!.Message
                : multiGetResponse.DebugInformation;
            throw new InvalidOperationException($"Failed to fetch courses for reorder from Elasticsearch. Reason: {errorMessage}");
        }

        if (multiGetResponse.Docs.Count != request.OrderedCourseIds.Count)
        {
            throw new KeyNotFoundException("One or more courses were not found in search index during reorder.");
        }

        var docs = multiGetResponse.Docs.ToList();
        var currentOrderIndexByCourseId = new Dictionary<Guid, int>(request.OrderedCourseIds.Count);
        for (var i = 0; i < request.OrderedCourseIds.Count; i++)
        {
            var orderIndex = docs[i].Match(
                getResult => getResult!.Source?.OrderIndex,
                _ => null);

            if (!orderIndex.HasValue)
            {
                throw new KeyNotFoundException($"Course with ID {request.OrderedCourseIds[i]} not found in search index.");
            }

            currentOrderIndexByCourseId[request.OrderedCourseIds[i]] = orderIndex.Value;
        }

        // Preserve existing slot values and only remap course IDs into those slots following the drag-drop order.
        var targetOrderIndexes = currentOrderIndexByCourseId.Values
            .OrderBy(orderIndex => orderIndex)
            .ToArray();

        var bulkRequest = new BulkRequest();
        var operations = new List<IBulkOperation>();

        for (int i = 0; i < request.OrderedCourseIds.Count; i++)
        {
            operations.Add(new BulkUpdateOperation<CourseSearch, object>(request.OrderedCourseIds[i])
            {
                Index = ConstantIndexElastic.CourseIndex,
                Doc = new { OrderIndex = targetOrderIndexes[i], request.UpdatedAt }
            });
        }

        bulkRequest.Operations = operations;
        var bulkResponse = await _elasticClient.BulkAsync(bulkRequest, cancellationToken);

        if (!bulkResponse.Errors)
        {
            return Unit.Value;
        }

        var failedItems = bulkResponse.ItemsWithErrors
            .Select(x => x.Error?.Reason)
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .ToList();
        var reasonText = failedItems.Count > 0
            ? string.Join("; ", failedItems)
            : (bulkResponse.TryGetOriginalException(out var ex) ? ex!.Message : bulkResponse.DebugInformation);

        throw new InvalidOperationException($"Failed to reorder courses in Elasticsearch. Errors: {reasonText}");
    }
}
#endregion