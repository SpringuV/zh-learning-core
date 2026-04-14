namespace Search.Application.EventHandlers.Lesson.Exercise;

public class ExerciseCreatedEventHandler(
        IExerciseSearchQueriesService exerciseSearchService, 
        ICacheVersionService cacheVersionService,
        ILogger<ExerciseCreatedEventHandler> logger) 
    : IIntegrationEventHandler<ExerciseCreatedIntegrationEvent>
{
    private readonly IExerciseSearchQueriesService _exerciseSearchService = exerciseSearchService;
    private readonly ICacheVersionService _cacheVersionService = cacheVersionService;
    private readonly ILogger<ExerciseCreatedEventHandler> _logger = logger;

    public async Task HandleAsync(ExerciseCreatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Indexing exercise {ExerciseId} into Elasticsearch", @event.ExerciseId);
        var request = new ExerciseSearchIndexQueriesRequest(
            ExerciseId: @event.ExerciseId,
            TopicId: @event.TopicId,
            Question: @event.Question,
            Description: @event.Description,
            OrderIndex: @event.OrderIndex,
            ExerciseType: @event.ExerciseType.ToString(),
            CorrectAnswer: @event.CorrectAnswer,
            SkillType: @event.SkillType.ToString(),
            Context: @event.Context.ToString(),
            Difficulty: @event.Difficulty.ToString(),
            Slug: @event.Slug,
            AudioUrl: @event.AudioUrl,
            ImageUrl: @event.ImageUrl,
            Explanation: @event.Explanation,
            IsPublished: @event.IsPublished,
            CreatedAt: @event.CreatedAt,
            UpdatedAt: @event.UpdatedAt,
            Options: [.. @event.Options.Select(o => new ExerciseOptionIndexDTOs(o.Id, o.Text))]
        );

        await _exerciseSearchService.IndexAsync(request, ct);
        await _cacheVersionService.InvalidateScopeAsync(SearchCacheScopes.ExerciseAdminSearch, ct);
        _logger.LogInformation("Exercise {ExerciseId} indexed successfully in Elasticsearch", @event.ExerciseId);
    }
}