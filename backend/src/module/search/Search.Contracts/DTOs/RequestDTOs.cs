namespace Search.Contracts.DTOs;

/// <summary>
/// Request to index an assignment in Elasticsearch
/// Contains all denormalized data needed for search
/// </summary>
public sealed record AssignmentSearchIndexQueriesRequest(
    Guid Id,
    Guid ClassroomId,
    Guid TeacherId,
    string Title,
    string Description,
    string AssignmentType,  // AllClass, Individual
    string SkillFocus,      // Reading, Writing, Listening, Speaking
    DateTime DueDate,
    bool IsPublished,
    int ExerciseCount,
    int RecipientCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record UserSearchIndexQueriesRequest(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt);

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

/// <summary>
/// Incremental update request for assignment in Elasticsearch
/// Used to sync partial changes without reindexing entire document
/// </summary>
public sealed record AssignmentSearchPatchDocumentRequest(
    string? Title = null,
    string? Description = null,
    string? SkillFocus = null,
    DateTime? DueDate = null,
    bool? IsPublished = null,
    int? ExerciseCount = null,
    int? RecipientCount = null,
    DateTime? UpdatedAt = null);

public sealed record UserSearchPatchDocumentRequest(
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    DateTime? LastLogin = null,
    int? CurrentLevel = null,
    DateTime? LastActivityAt = null,
    string? AvatarUrl = null,
    DateTime? UpdatedAt = null);

public sealed record UserSearchQueryRequest(
    string? Email = null,
    string? Username = null,
    bool? IsActive = null,
    string? PhoneNumber = null,
    int Take = 30,
    // keyset pagination sẽ dùng SearchAfter, sẽ lấy những document có CreatedAt nhỏ hơn timestamp của document cuối cùng trong page trước, để tránh việc skip nhiều document khi trang có nhiều kết quả
    string? SearchAfterCreatedAt = null, // dùng để phân trang, timestamp của document cuối cùng trong page trước, sẽ lấy những document có CreatedAt nhỏ hơn timestamp này
    UserSortBy SortBy = UserSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null);

public enum UserSortBy
{
    CreatedAt,
    UpdatedAt,
    CurrentLevel,
    Email,
    Username,
}

