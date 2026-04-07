namespace Search.Contracts.DTOs;

/// <summary>
/// Query request for searching assignments
/// Keyset pagination using SearchAfterCreatedAt to avoid offset inefficiency
/// </summary>
public sealed record AssignmentSearchQueryRequest(
    Guid? ClassroomId = null,
    Guid? TeacherId = null,
    string? SkillFocus = null,  // Reading, Writing, Listening, Speaking
    bool? IsPublished = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    string? SearchQuery = null,  // Full-text search on Title + Description
    int Take = 30,
    string? SearchAfterCreatedAt = null,  // Keyset pagination cursor (CreatedAt from last item)
    AssignmentSortBy SortBy = AssignmentSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null);

/// <summary>
/// Sort options for assignment search
/// </summary>
public enum AssignmentSortBy
{
    CreatedAt,
    UpdatedAt,
    DueDate,
    Title,
}
