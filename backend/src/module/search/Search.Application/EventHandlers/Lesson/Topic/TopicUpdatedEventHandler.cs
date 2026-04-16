namespace Search.Application.EventHandlers.Lesson.Topic;

#region Publish/Unpublish
public class TopicPublishedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicPublishedEventHandler> logger) 
    : IIntegrationEventHandler<TopicPublishedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicPublishedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicPublishedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating publish topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicPublishedRequestDTO(
            TopicId: @event.TopicId,
            UpdatedAt: @event.PublishedAt
        );

        await _topicSearchService.PublishAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the published topic
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Publish topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
    }
}

public class TopicUnPublishedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicUnPublishedEventHandler> logger) 
    : IIntegrationEventHandler<TopicUnpublishedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicUnPublishedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicUnpublishedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating unpublish topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicUnPublishedRequestDTO(
            TopicId: @event.TopicId,
            UpdatedAt: @event.UnpublishedAt
        );

        await _topicSearchService.UnPublishAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the unpublished topic
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Unpublish topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
    }
}
#endregion
#region ReOrder Topics
public class TopicReOrderedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicReOrderedEventHandler> logger) 
    : IIntegrationEventHandler<TopicReOrderedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicReOrderedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicReOrderedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating topic order for course {CourseId} in Elasticsearch", @event.CourseId);
        var request = new TopicReorderSearchRequestDTO(
            CourseId: @event.CourseId,
            OrderedTopicIds: @event.OrderedTopicIds,
            UpdatedAt: @event.UpdatedAt
        );

        await _topicSearchService.ReOrderAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the updated topic order
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Topic order for course {CourseId} updated successfully in Elasticsearch", @event.CourseId);
    }
}
#endregion

#region Title
public class TopicTitleUpdatedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicTitleUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<TopicTitleUpdatedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicTitleUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicTitleUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating title for topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicTitleUpdatedRequestDTO(
            TopicId: @event.TopicId,
            CourseId: @event.CourseId,
            NewTitle: @event.NewTitle,
            NewSlug: @event.NewSlug,
            UpdatedAt: @event.UpdatedAt
         );

        await _topicSearchService.UpdateTitleAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the updated topic title
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Title for topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
     }
}
#endregion

#region Description
public class TopicDescriptionUpdatedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicDescriptionUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<TopicDescriptionUpdatedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicDescriptionUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicDescriptionUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating description for topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicDescriptionUpdatedRequestDTO(
            TopicId: @event.TopicId,
            CourseId: @event.CourseId,
            NewDescription: @event.NewDescription,
            UpdatedAt: @event.UpdatedAt
         );

        await _topicSearchService.UpdateDescriptionAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the updated topic description
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Description for topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
     }
}
#endregion

#region EstimatedTime
public class TopicEstimatedTimeUpdatedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicEstimatedTimeUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<TopicEstimatedTimeUpdatedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicEstimatedTimeUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicEstimatedTimeUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating estimated time for topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicEstimatedTimeUpdatedRequestDTO(
            TopicId: @event.TopicId,
            CourseId: @event.CourseId,
            NewEstimatedTimeMinutes: @event.NewEstimatedTimeMinutes,
            UpdatedAt: @event.UpdatedAt
         );

        await _topicSearchService.UpdateEstimatedTimeAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the updated topic estimated time
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Estimated time for topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
     }
}
#endregion
#region ExamInfo
public class TopicExamInfoUpdatedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicExamInfoUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<TopicExamInfoUpdatedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicExamInfoUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicExamInfoUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating exam info for topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicExamInfoUpdatedRequestDTO(
            TopicId: @event.TopicId,
            CourseId: @event.CourseId,
            NewExamYear: @event.NewExamYear,
            NewExamCode: @event.NewExamCode,
            UpdatedAt: @event.UpdatedAt
         );

        await _topicSearchService.UpdateExamInfoAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the updated topic exam info
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Exam info for topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
     }
}
#endregion
#region TotalExercises
public class TopicTotalExercisesUpdatedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicTotalExercisesUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<TopicTotalExercisesUpdatedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicTotalExercisesUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicTotalExercisesUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating total exercises for topic {TopicId} in Elasticsearch", @event.TopicId);
        var request = new TopicTotalExercisesUpdatedRequestDTO(
            TopicId: @event.TopicId,
            TotalExercises: @event.TotalExercises,
            UpdatedAt: @event.UpdatedAt
        );

        await _topicSearchService.UpdateTotalExercisesAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the updated topic total exercises
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Total exercises for topic {TopicId} updated successfully in Elasticsearch", @event.TopicId);
    }
}
#endregion

#region Delete
public class TopicDeletedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<TopicDeletedEventHandler> logger) 
    : IIntegrationEventHandler<TopicDeletedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<TopicDeletedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicDeletedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Deleting topic {TopicId} from Elasticsearch", @event.TopicId);
        var request = new TopicDeletedRequestDTO(
            TopicId: @event.TopicId
        );
        await _topicSearchService.DeleteAsync(request, ct);
        // Invalidate cache for topic admin search to reflect the deleted topic
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.TopicAdminSearch, ct);
        _logger.LogInformation("Topic {TopicId} deleted successfully from Elasticsearch", @event.TopicId);
    }
}
#endregion