using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface IAssignmentSearchQueriesService
{
    Task<SearchQueryResult<AssignmentSearchItemResponse>> SearchAssignmentsAsync(
        AssignmentSearchQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<AssignmentSearchItemResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<AssignmentIndexResponse> IndexAsync(
        AssignmentSearchIndexQueriesRequest request,
        CancellationToken cancellationToken = default);

    Task<AssignmentSearchPatchDocumentResponse?> PatchAsync(
        Guid id,
        AssignmentSearchPatchDocumentRequest patch,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
