using Search.Infrastructure.Queries.Lesson.Indexs;

namespace Search.Infrastructure.Services;

public class CourseSearchQueriesServices(ILogger<CourseSearchQueriesServices> logger, IMediator mediator) : ICourseSearchQueriesService
{
    private readonly ILogger<CourseSearchQueriesServices> _logger = logger;
    private readonly IMediator _mediator = mediator;
    public Task<CourseIndexResponse> IndexAsync(CourseSearchIndexQueriesRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CourseIndexCommand(
            Id: request.CourseId,
            Title: request.Title,
            Description: request.Description,
            OrderIndex: request.OrderIndex,
            HskLevel: request.HskLevel,
            Slug: request.Slug,
            TotalTopics: request.TotalTopics,
            TotalStudentsEnrolled: request.TotalStudentsEnrolled,
            IsPublished: request.IsPublished,
            CreatedAt: request.CreatedAt,
            UpdatedAt: request.UpdatedAt);
        return _mediator.Send(command, cancellationToken);

    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SearchQueryResult<CourseSearchItemAdminResponse>> GetCourseSearchItemAdminAsync(CourseSearchQueryAdminRequest request, CancellationToken cancellationToken = default)
    {
        var query = new CourseSearchAdminQueries(
            Title: request.Title,
            StartCreatedAt: request.StartCreatedAt,
            EndCreatedAt: request.EndCreatedAt,
            SearchAfterValues: request.SearchAfterValues,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending,
            Take: request.Take);
        return _mediator.Send(query, cancellationToken);

    }
}