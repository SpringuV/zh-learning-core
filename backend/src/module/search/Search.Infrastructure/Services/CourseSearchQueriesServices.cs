using Search.Infrastructure.Queries.Lesson.Delete;
using Search.Infrastructure.Queries.Lesson.Update.Course;

namespace Search.Infrastructure.Services;

public class CourseSearchQueriesServices(ILogger<CourseSearchQueriesServices> logger, IMediator mediator) : ICourseSearchQueriesService
{
    private readonly ILogger<CourseSearchQueriesServices> _logger = logger;
    private readonly IMediator _mediator = mediator;
    public async Task<CourseIndexResponse> IndexAsync(CourseSearchIndexQueriesRequest request, CancellationToken cancellationToken = default)
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
        return await _mediator.Send(command, cancellationToken);

    }

    public async Task DeleteAsync(CourseDeletedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseDeletedSearchCommand(
            CourseId: request.CourseId
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task<SearchQueryResult<CourseSearchItemAdminResponse>> GetCourseSearchItemAdminAsync(CourseSearchQueryAdminRequest request, CancellationToken cancellationToken = default)
    {
        var query = new CourseSearchAdminQueries(
            Title: request.Title,
            StartCreatedAt: request.StartCreatedAt,
            EndCreatedAt: request.EndCreatedAt,
            SearchAfterValues: request.SearchAfterValues,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending,
            Take: request.Take);
        return await _mediator.Send(query, cancellationToken);
    }

    public async Task PublishAsync(CoursePublishedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CoursePublishedSearchCommand(
            CourseId: request.CourseId,
            PublishedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UnPublishAsync(CourseUnPublishedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseUnPublishedSearchCommand(
            CourseId: request.CourseId,
            UnpublishedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task ReOrderAsync(CourseReorderSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseReOrderedSearchCommand(
            OrderedCourseIds: request.OrderedCourseIds,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UpdateTotalTopicsAsync(CourseTotalTopicsUpdatedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseTotalTopicsUpdatedSearchCommand(
            CourseId: request.CourseId,
            TotalTopics: request.TotalTopics,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UpdateTitleAsync(CourseTitleUpdatedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseTitleUpdatedSearchCommand(
            CourseId: request.CourseId,
            NewTitle: request.Title,
            NewSlug: request.Slug,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UpdateDescriptionAsync(CourseDescriptionUpdatedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseDescriptionUpdatedSearchCommand(
            CourseId: request.CourseId,
            NewDescription: request.NewDescription,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UpdateHskLevelAsync(CourseHskLevelUpdatedSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new CourseHskLevelUpdatedSearchCommand(
            CourseId: request.CourseId,
            NewHskLevel: request.HskLevel,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }
}