using Search.Infrastructure.Queries.Lesson.Delete;
using Search.Infrastructure.Queries.Lesson.Search.Detail;
using Search.Infrastructure.Queries.Lesson.Update.Topic;

namespace Search.Infrastructure.Services;

public class TopicSearchQueriesService(IMediator mediator) : ITopicSearchQueriesService
{
    private IMediator _mediator = mediator;

    private Task SendPatchAsync(Guid topicId, object patch, CancellationToken cancellationToken)
        => _mediator.Send(new TopicPatchSearchCommand(topicId, patch), cancellationToken);

    #region Title Update
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
    #endregion
    #region Description Update
    public Task UpdateDescriptionAsync(TopicDescriptionUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.TopicId,
            new
            {
                Description = request.NewDescription,
                request.UpdatedAt
            },
            cancellationToken);
    #endregion
    #region EstimatedTime Update
    public Task UpdateEstimatedTimeAsync(TopicEstimatedTimeUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.TopicId,
            new
            {
                EstimatedTimeMinutes = request.NewEstimatedTimeMinutes,
                request.UpdatedAt
            },
            cancellationToken);
    #endregion
    #region ExamInfo Update
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
    #endregion
    #region IndexAsync
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
    #endregion
    #region DeleteAsync
    public async Task DeleteAsync(TopicDeletedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicDeletedSearchCommand(
            TopicId: request.TopicId
        );
        await _mediator.Send(command, cancellationToken);
    }
    #endregion
    #region SearchTopicAdmin
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
            Page: request.Page,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending
        );
        return await _mediator.Send(queries, cancellationToken);

    }
    #endregion
    #region SearchMetadata
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
            Page: request.Page,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending
        );
        return await _mediator.Send(queries, cancellationToken);
    }
    #endregion
    #region Publish/Unpublish
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
    #endregion
    #region ReOrder
    public async Task ReOrderAsync(TopicReorderSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicReOrderSearchCommand(
            CourseId: request.CourseId,
             OrderedTopicIds: request.OrderedTopicIds,
             UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }
    #endregion
    #region UpdateTotalExercises
    public async Task UpdateTotalExercisesAsync(TopicTotalExercisesUpdatedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new TopicTotalExercisesUpdatedSearchCommand(
            TopicId: request.TopicId,
            TotalExercises: request.TotalExercises,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }
    #endregion

    #region Detail
    public async Task<TopicSearchDetailResponse> GetTopicDetailSearchItemAdminAsync(Guid topicId, CancellationToken cancellationToken = default)
    {
        var query = new TopicSearchDetailAdminQueries(topicId);
        return await _mediator.Send(query, cancellationToken);
    }
    #endregion
}