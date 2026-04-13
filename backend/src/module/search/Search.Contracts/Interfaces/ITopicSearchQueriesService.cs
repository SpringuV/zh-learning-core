using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface ITopicSearchQueriesService
{
    Task<TopicIndexResponse> IndexAsync(TopicSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SearchQueryResult<TopicSearchItemAdminResponse>> SearchTopicAdminAsync(TopicSearchQueryRequest request, CancellationToken cancellationToken = default);
}