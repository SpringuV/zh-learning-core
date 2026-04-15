namespace Search.Application.EventHandlers.Lesson.Course;

#region TotalTopicsUpdated
public class CourseTotalTopicsUpdatedEventHandler(
        ICourseSearchQueriesService courseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<CourseTotalTopicsUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<CourseTotalTopicsUpdatedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseTotalTopicsUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseTotalTopicsUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new CourseTotalTopicsUpdatedSearchRequestDTO(
            CourseId: @event.CourseId,
            TotalTopics: @event.TotalTopics,
            UpdatedAt: @event.UpdatedAt
        );

        await _courseSearchService.UpdateTotalTopicsAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("Course {CourseId} updated successfully in Elasticsearch", @event.CourseId);
    }
}
#endregion

#region Publish/Unpublish
public class CoursePublishedEventHandler(
        ICourseSearchQueriesService courseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<CoursePublishedEventHandler> logger) 
    : IIntegrationEventHandler<CoursePublishedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CoursePublishedEventHandler> _logger = logger;

    public async Task HandleAsync(CoursePublishedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating publish course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new CoursePublishedSearchRequestDTO(
            CourseId: @event.CourseId,
            UpdatedAt: @event.UpdatedAt
        );

        await _courseSearchService.PublishAsync(request, ct);
        // Invalidate cache for course admin search to reflect the published course
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("Publish course {CourseId} updated successfully in Elasticsearch", @event.CourseId);
    }
}

public class CourseUnPublishedEventHandler(
        ICourseSearchQueriesService courseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<CourseUnPublishedEventHandler> logger) 
    : IIntegrationEventHandler<CourseUnpublishedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseUnPublishedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseUnpublishedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating unpublish course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new CourseUnPublishedSearchRequestDTO(
            CourseId: @event.CourseId,
            UpdatedAt: @event.UpdatedAt
        );

        await _courseSearchService.UnPublishAsync(request, ct);
        // Invalidate cache for course admin search to reflect the unpublished course
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("Unpublish course {CourseId} updated successfully in Elasticsearch", @event.CourseId);
    }
}
#endregion

#region ReOrder
public class CourseReOrderedEventHandler(
        ICourseSearchQueriesService courseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<CourseReOrderedEventHandler> logger) 
    : IIntegrationEventHandler<CourseReOrderedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseReOrderedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseReOrderedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Reorder Course {event} in Elasticsearch", @event);
        var request = new CourseReorderSearchRequestDTO(
            OrderedCourseIds: @event.OrderedCourseIds,
            UpdatedAt: @event.UpdatedAt
        );

        await _courseSearchService.ReOrderAsync(request, ct);
        // Invalidate cache for course admin search to reflect the new order index
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("ReOrder Course updated successfully in Elasticsearch");
    }
}
#endregion

#region Title
public sealed class CourseTitleUpdatedEventHandler(
        ICourseSearchQueriesService courseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<CourseTitleUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<CourseTitleUpdatedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseTitleUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseTitleUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating title for course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new CourseTitleUpdatedSearchRequestDTO(
            CourseId: @event.CourseId,
            Title: @event.NewTitle,
            Slug: @event.NewSlug,
            UpdatedAt: @event.UpdatedAt
        );
        await _courseSearchService.UpdateTitleAsync(request, ct);
        // Invalidate cache for course admin search to reflect the updated title
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("Title for course {CourseId} updated successfully in Elasticsearch", @event.CourseId);

    }
}
#endregion
#region Description
public sealed class CourseDescriptionUpdatedEventHandler(
        ICourseSearchQueriesService courseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<CourseDescriptionUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<CourseDescriptionUpdatedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseDescriptionUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseDescriptionUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating description for course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new CourseDescriptionUpdatedSearchRequestDTO(
            CourseId: @event.CourseId,
            NewDescription: @event.NewDescription,
            UpdatedAt: @event.UpdatedAt
        );
        await _courseSearchService.UpdateDescriptionAsync(request, ct);
        // Invalidate cache for course admin search to reflect the updated description
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("Description for course {CourseId} updated successfully in Elasticsearch", @event.CourseId);

    }
}
#endregion

#region HskLevel
public sealed class CourseHskLevelUpdatedEventHandler(
        ICourseSearchQueriesService courseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<CourseHskLevelUpdatedEventHandler> logger)
    : IIntegrationEventHandler<CourseHskLevelUpdatedIntegrationEvent>
{
    private readonly ICourseSearchQueriesService _courseSearchService = courseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<CourseHskLevelUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(CourseHskLevelUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating HSK level for course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new CourseHskLevelUpdatedSearchRequestDTO(
            CourseId: @event.CourseId,
            HskLevel: @event.NewHskLevel,
            UpdatedAt: @event.UpdatedAt
        );
        await _courseSearchService.UpdateHskLevelAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.CourseAdminSearch, ct);
        _logger.LogInformation("HSK level for course {CourseId} updated successfully in Elasticsearch", @event.CourseId);
    }
}
#endregion