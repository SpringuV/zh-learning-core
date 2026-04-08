
using Search.Contracts.DTOs;
namespace Search.Contracts.Interfaces;

/*
[Controller/API]
    ↓
[Service - Adapter]  (IndexAsync, PatchAsync, SearchAsync, GetAsync, DeleteAsync)
    ↓
[Mediator.Send]
    ↓
[Handler - Business Logic] (Handler chứa validation, Elasticsearch, logging)
    ↓
[ElasticsearchClient]
*/

public interface IUserSearchQueriesServices
{
	Task<SearchQueryResult<UserSearchItemResponse>> SearchUsersAsync(UserSearchQueryRequest request, CancellationToken cancellationToken = default);
    Task<UserSearchResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserIndexResponse> IndexAsync(UserSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    Task<UserSearchPatchDocumentResponse?> PatchAsync(Guid id, UserSearchPatchDocumentRequest patch, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

