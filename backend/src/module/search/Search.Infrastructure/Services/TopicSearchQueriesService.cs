using Search.Infrastructure.Queries.Lesson.Update.Topic;

namespace Search.Infrastructure.Services;

public class TopicSearchQueriesService(IMediator mediator) : ITopicSearchQueriesService
{
    private IMediator _mediator = mediator;

    private Task SendPatchAsync(Guid topicId, object patch, CancellationToken cancellationToken)
        => _mediator.Send(new TopicPatchSearchCommand(topicId, patch), cancellationToken);

    public Task UpdateTitleAsync(TopicTitleUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.TopicId,
            new
            {
                Title = request.NewTitle,
                Slug = request.NewSlug,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateDescriptionAsync(TopicDescriptionUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.TopicId,
            new
            {
                Description = request.NewDescription,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateEstimatedTimeAsync(TopicEstimatedTimeUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.TopicId,
            new
            {
                EstimatedTimeMinutes = request.NewEstimatedTimeMinutes,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateExamInfoAsync(TopicExamInfoUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.TopicId,
            new
            {
                ExamYear = request.NewExamYear,
                request.NewExamCode,
                request.UpdatedAt
            },
            cancellationToken);

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

    public async Task<TopicSearchWithCourseMetadataResponse> SearchTopicAdminWithCourseMetadataAsync(TopicSearchQueryRequest request, CancellationToken cancellationToken = default)
    {
        var queries = new CourseTopicsOverviewAdminQueries(
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

    public async Task UnPublishAsync(TopicUnPublishedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicUnPublishedSearchCommand(
            TopicId: request.TopicId,
            UnpublishedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task PublishAsync(TopicPublishedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicPublishedSearchCommand(
            TopicId: request.TopicId,
            PublishedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task ReOrderAsync(TopicReorderSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicReOrderSearchCommand(
            CourseId: request.CourseId,
             OrderedTopicIds: request.OrderedTopicIds,
             UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UpdateTotalExercisesAsync(TopicTotalExercisesUpdatedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicTotalExercisesUpdatedSearchCommand(
            TopicId: request.TopicId,
            TotalExercises: request.TotalExercises,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }
}