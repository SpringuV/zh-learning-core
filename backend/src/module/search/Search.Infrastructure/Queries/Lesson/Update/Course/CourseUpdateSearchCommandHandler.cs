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
        var bulkRequest = new BulkRequest();
        var operations = new List<IBulkOperation>();

        for (int i = 0; i < request.OrderedCourseIds.Count; i++)
        {
            // Sử dụng Bulk API để cập nhật trường OrderIndex cho mỗi khóa học dựa trên vị trí của nó trong OrderedCourseIds
            // các Id sẽ được truyền qua constructor của BulkUpdateOperation, đảm bảo rằng mỗi operation sẽ cập nhật đúng document tương ứng trong Elasticsearch
            operations.Add(new BulkUpdateOperation<CourseSearch, object>(request.OrderedCourseIds[i])
            {
                Index = ConstantIndexElastic.CourseIndex,
                Doc = new { OrderIndex = i + 1, request.UpdatedAt }
            });
        }
        // Gán danh sách các operation vào BulkRequest để thực hiện một lần gọi duy nhất đến Elasticsearch, tối ưu hiệu suất và giảm thiểu số lượng request cần thiết
        bulkRequest.Operations = operations;
        var response = await _elasticClient.BulkAsync(bulkRequest, cancellationToken);

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

        throw new InvalidOperationException($"Failed to reorder courses in Elasticsearch. Errors: {reasonText}");
    }
}
#endregion