namespace Search.Application.EventHandlers.Lesson.Exercise;

#region UnPublish
public class ExerciseUnPublishedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseUnPublishedEventHandler> logger) 
    : IIntegrationEventHandler<ExerciseUnpublishedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseUnPublishedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseUnpublishedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Indexing unpublished exercise {ExerciseId} into Elasticsearch", @event.ExerciseId);
        var request = new ExerciseUnPublishedRequestDTO(
            ExerciseId: @event.ExerciseId,
            UpdatedAt: @event.UnpublishedAt
        );

        await _exerciseSearchService.UnPublishAsync(request, ct);
        // Invalidate cache for exercise admin search to reflect the newly published exercise
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Unpublished exercise {ExerciseId} indexed successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region ReOrder
public class ExerciseReOrderedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseReOrderedEventHandler> logger) 
    : IIntegrationEventHandler<ExerciseReOrderedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseReOrderedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseReOrderedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Reorder Exercise {event} in Elasticsearch", @event);
        var request = new ExerciseReorderSearchRequestDTO(
            TopicId: @event.TopicId,
            OrderedExerciseIds: @event.OrderedExerciseIds,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.ReOrderAsync(request, ct);
        // Invalidate cache for exercise admin search to reflect the new order index
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("ReOrder Exercise updated successfully in Elasticsearch");
    }
}
#endregion

#region Publish
public class ExercisePublishedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExercisePublishedEventHandler> logger) 
    : IIntegrationEventHandler<ExercisePublishedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExercisePublishedEventHandler> _logger = logger;

    public async Task HandleAsync(ExercisePublishedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Indexing published exercise {ExerciseId} into Elasticsearch", @event.ExerciseId);
        var request = new ExercisePublishedRequestDTO(
            ExerciseId: @event.ExerciseId,
            UpdatedAt: @event.PublishedAt
        );

        await _exerciseSearchService.PublishAsync(request, ct);
        // Invalidate cache for exercise admin search to reflect the newly published exercise
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Published exercise {ExerciseId} indexed successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion
#region ExerciseType
public class ExerciseTypeUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseTypeUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<ExerciseTypeUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseTypeUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseTypeUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating ExerciseType for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseTypeUpdatedRequestDTO(
            TopicId: @event.TopicId,
            ExerciseId: @event.ExerciseId,
            NewExerciseType: @event.NewExerciseType,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateExerciseTypeAsync(request, ct);
        // Invalidate cache for exercise admin search to reflect the updated exercise type
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("ExerciseType for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region SkillType
public class ExerciseSkillTypeUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseSkillTypeUpdatedEventHandler> logger) 
    : IIntegrationEventHandler<ExerciseSkillTypeUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseSkillTypeUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseSkillTypeUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating SkillType for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseSkillTypeUpdatedRequestDTO(
            TopicId: @event.TopicId,
            ExerciseId: @event.ExerciseId,
            NewSkillType: @event.NewSkillType,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateExerciseSkillTypeAsync(request, ct);
        // Invalidate cache for exercise admin search to reflect the updated skill type
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("SkillType for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region Context
public class ExerciseContextUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseContextUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseContextUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseContextUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseContextUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Context for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseContextUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewContext: @event.NewContext,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateExerciseContextAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Context for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region Description
public class ExerciseDescriptionUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseDescriptionUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseDescriptionUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseDescriptionUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseDescriptionUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Description for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseDescriptionUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewDescription: @event.NewDescription,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateDescriptionAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Description for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region Question
public class ExerciseQuestionUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseQuestionUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseQuestionUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseQuestionUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseQuestionUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Question for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseQuestionUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewQuestion: @event.NewQuestion,
            NewSlug: @event.NewSlug,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateQuestionAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Question for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region CorrectAnswer
public class ExerciseCorrectAnswerUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseCorrectAnswerUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseCorrectAnswerUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseCorrectAnswerUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseCorrectAnswerUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating CorrectAnswer for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseCorrectAnswerUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewCorrectAnswer: @event.NewCorrectAnswer,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateCorrectAnswerAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("CorrectAnswer for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region Difficulty
public class ExerciseDifficultyUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseDifficultyUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseDifficultyUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseDifficultyUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseDifficultyUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Difficulty for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseDifficultyUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewDifficulty: @event.NewDifficulty,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateDifficultyAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Difficulty for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region AudioUrl
public class ExerciseAudioUrlUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseAudioUrlUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseAudioUrlUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseAudioUrlUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseAudioUrlUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating AudioUrl for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseAudioUrlUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewAudioUrl: @event.NewAudioUrl,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateAudioUrlAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("AudioUrl for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region ImageUrl
public class ExerciseImageUrlUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseImageUrlUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseImageUrlUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseImageUrlUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseImageUrlUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating ImageUrl for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseImageUrlUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewImageUrl: @event.NewImageUrl,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateImageUrlAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("ImageUrl for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region Explanation
public class ExerciseExplanationUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseExplanationUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseExplanationUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseExplanationUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseExplanationUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Explanation for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseExplanationUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewExplanation: @event.NewExplanation,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateExplanationAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Explanation for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion

#region Options
public class ExerciseOptionsUpdatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService,
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseOptionsUpdatedEventHandler> logger)
    : IIntegrationEventHandler<ExerciseOptionsUpdatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseOptionsUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseOptionsUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Updating Options for exercise {ExerciseId} in Elasticsearch", @event.ExerciseId);
        var request = new ExerciseOptionsUpdatedRequestDTO(
            ExerciseId: @event.ExerciseId,
            NewOptions: @event.NewOptions,
            UpdatedAt: @event.UpdatedAt
        );

        await _exerciseSearchService.UpdateOptionsAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Options for exercise {ExerciseId} updated successfully in Elasticsearch", @event.ExerciseId);
    }
}
#endregion
