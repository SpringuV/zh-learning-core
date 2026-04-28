namespace Search.Application.EventHandlers.Lesson.Course;

public class CourseCreatedEventHandler(
        ICourseSearchQueriesService courseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<CourseCreatedEventHandler> logger) 
    : IIntegrationEventHandler<CourseCreatedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseCreatedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseCreatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Indexing course {CourseId} into Elasticsearch", @event.CourseId);
        var request = new CourseSearchIndexQueriesRequest(
            CourseId: @event.CourseId,
            Title: @event.Title,
            Description: @event.Description,
            OrderIndex: @event.OrderIndex,
            HskLevel: @event.HskLevel,
            Slug: @event.Slug,
            TotalTopicsPublished: @event.TotalTopicsPublished,
            TotalTopics: @event.TotalTopics,
            TotalStudentsEnrolled: @event.TotalStudentsEnrolled,
            IsPublished: @event.IsPublished,
            CreatedAt: @event.CreatedAt,
            UpdatedAt: @event.UpdatedAt
        );

        await _courseSearchService.IndexAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("Course {CourseId} indexed successfully in Elasticsearch", @event.CourseId);
    }
}