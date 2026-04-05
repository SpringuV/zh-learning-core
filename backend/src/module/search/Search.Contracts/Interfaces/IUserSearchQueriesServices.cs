
using HanziAnhVu.Shared.Application;
using Search.Contracts.DTOs;
using Search.Domain.Entities;

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
    Task<UserSearch?> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<UserIndexResponse> IndexAsync(UserSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    Task<UserSearchPatchDocumentResponse?> PatchAsync(string id, UserSearchPatchDocumentRequest patch, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
public enum UserSortBy
{
    CreatedAt,
    UpdatedAt,
    CurrentLevel,
}


