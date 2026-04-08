using HanziAnhVu.Shared.Application;
using Search.Contracts.DTOs;

namespace Search.Application.Interfaces;

/// <summary>
/// IAssignmentSearchQueriesService - Search operations for assignments (CQRS Read Side)
/// Uses Elasticsearch for fast searching
/// 
/// Adapter flow:
/// [Controller/API]
///    ↓
/// [Service - Adapter] (IndexAsync, PatchAsync, SearchAsync, GetAsync, DeleteAsync)
///    ↓
/// [Mediator.Send]
///    ↓
/// [Handler - Business Logic] (Handler contains validation, Elasticsearch, logging)
///    ↓
/// [ElasticsearchClient]
/// </summary>
public interface IAssignmentSearchQueriesService
{
    /// <summary>
    /// Search assignments with filters and keyset pagination
    /// </summary>
    Task<SearchQueryResult<AssignmentSearchItemResponse>> SearchAssignmentsAsync(
        AssignmentSearchQueryRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single assignment document by ID
    /// </summary>
    Task<AssignmentSearchItemResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Index (create or update) assignment in Elasticsearch
    /// Used when assignment is created or published
    /// </summary>
    Task<AssignmentIndexResponse> IndexAsync(
        AssignmentSearchIndexQueriesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Partially update assignment document in Elasticsearch
    /// Used when assignment fields change (title, due date, published status, etc.)
    /// </summary>
    Task<AssignmentSearchPatchDocumentResponse?> PatchAsync(
        Guid id,
        AssignmentSearchPatchDocumentRequest patch,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete assignment from Elasticsearch
    /// Used when assignment is deleted
    /// </summary>
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
