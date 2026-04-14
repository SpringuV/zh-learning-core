namespace Search.Infrastructure.Services;

public class TopicSearchQueriesService(IMediator mediator) : ITopicSearchQueriesService
{
    private IMediator _mediator = mediator;
    public async Task<TopicIndexResponse> IndexAsync(TopicSearchIndexQueriesRequest request, CancellationToken cancellationToken = default)
    {
        var command = new TopicIndexCommand(
            TopicId: request.TopicId,
            CourseId: request.CourseId,
            Title: request.Title,
            Description: request.Description,
            OrderIndex: request.OrderIndex,
            TopicType: request.TopicType,
            EstimatedTimeMinutes: request.EstimatedTimeMinutes,
            ExamYear: request.ExamYear,
            ExamCode: request.ExamCode,
            Slug: request.Slug,
            IsPublished: request.IsPublished,
            TotalExercises: request.TotalExercises,
            CreatedAt: request.CreatedAt,
            UpdatedAt: request.UpdatedAt);
        return await _mediator.Send(command, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SearchQueryResult<TopicSearchItemAdminResponse>> SearchTopicAdminAsync(TopicSearchQueryRequest request, CancellationToken cancellationToken = default)
    {
        var queries = new TopicSearchAdminQueries(
            CourseId: request.CourseId,
            Title: request.Title,
            TopicType: request.TopicType,
            IsPublished: request.IsPublished,
            StartCreatedAt: request.StartCreatedAt,
            EndCreatedAt: request.EndCreatedAt,
            Take: request.Take,
            SearchAfterValues: request.SearchAfterValues,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending
        );
        return await _mediator.Send(queries, cancellationToken);

    }
}