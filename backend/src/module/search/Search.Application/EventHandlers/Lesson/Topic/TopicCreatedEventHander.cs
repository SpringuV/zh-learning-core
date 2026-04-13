using Lesson.Contracts;

namespace Search.Application.EventHandlers.Lesson.Topic;

public class TopicCreatedEventHandler(
        ITopicSearchQueriesService topicSearchService, 
        ILogger<TopicCreatedEventHandler> logger) 
    : IIntegrationEventHandler<TopicCreatedIntegrationEvent>
{
    private readonly ITopicSearchQueriesService _topicSearchService = topicSearchService;
    private readonly ILogger<TopicCreatedEventHandler> _logger = logger;

    public async Task HandleAsync(TopicCreatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Indexing topic {TopicId} into Elasticsearch", @event.TopicId);
        var request = new TopicSearchIndexQueriesRequest(
            TopicId: @event.TopicId,
            CourseId: @event.CourseId,
            Title: @event.Title,
            Description: @event.Description,
            OrderIndex: @event.OrderIndex,
            TopicType: @event.TopicType,
            EstimatedTimeMinutes: @event.EstimatedTimeMinutes,
            ExamYear: @event.ExamYear,
            ExamCode: @event.ExamCode,
            Slug: @event.Slug,
            IsPublished: @event.IsPublished,
            TotalExercises: @event.TotalExercises,
            CreatedAt: @event.CreatedAt,
            UpdatedAt: @event.UpdatedAt
        );

        await _topicSearchService.IndexAsync(request, ct);
        _logger.LogInformation("Topic {TopicId} indexed successfully in Elasticsearch", @event.TopicId);
    }
}