using HanziAnhVu.Shared.Contracts;
using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface ITopicSearchQueriesService
{
    Task<Result<IEnumerable<TopicSearchForDashboardClientResponse>>> GetTopicForDashboardClientAsync(string slug, CancellationToken cancellationToken = default);
    Task<TopicSearchDetailResponse> GetTopicDetailSearchItemAdminAsync(Guid topicId, CancellationToken cancellationToken = default);
    Task UpdateExamInfoAsync(TopicExamInfoUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateEstimatedTimeAsync(TopicEstimatedTimeUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateDescriptionAsync(TopicDescriptionUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task<TopicIndexResponse> IndexAsync(TopicSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    Task UpdateTitleAsync(TopicTitleUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UnPublishAsync(TopicUnPublishedRequestDTO request, CancellationToken cancellationToken = default);
    Task PublishAsync(TopicPublishedRequestDTO request, CancellationToken cancellationToken = default);
    Task ReOrderAsync(TopicReorderSearchRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateTotalExercisesAsync(TopicTotalExercisesUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task DeleteAsync(TopicDeletedRequestDTO request, CancellationToken cancellationToken = default);
    Task<SearchQueryResult<TopicSearchItemAdminResponse>> SearchTopicAdminAsync(TopicSearchQueryRequest request, CancellationToken cancellationToken = default);
    Task<TopicSearchWithCourseMetadataResponse> SearchTopicAdminWithCourseMetadataAsync(TopicSearchQueryRequest request, CancellationToken cancellationToken = default);
}