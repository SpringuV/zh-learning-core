using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface ICourseSearchQueriesService
{
    // Task<SearchQueryResult<CourseSearchItemResponse>> SearchCoursesAsync(CourseSearchQueryRequest request, CancellationToken cancellationToken = default);
    // Task<CourseSearchResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CourseIndexResponse> IndexAsync(CourseSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    // Task<CourseSearchPatchDocumentResponse?> PatchAsync(Guid id, CourseSearchPatchDocumentRequest patch, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SearchQueryResult<CourseSearchItemAdminResponse>> GetCourseSearchItemAdminAsync(CourseSearchQueryAdminRequest request, CancellationToken cancellationToken = default);
}