namespace Search.Contracts.DTOs;

public sealed record AssignmentIndexResponse(
    Guid AssignmentId, 
    DateTime IndexedAt);

/// <summary>
/// Denormalized DTO for assignment search results (read-optimized)
/// </summary>
public sealed record AssignmentSearchItemResponse(
    string Id,
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

public record UserIndexResponse(Guid UserId, DateTime IndexedAt);

/// <summary>
/// Response after patching assignment document in Elasticsearch
/// </summary>
public sealed record AssignmentSearchPatchDocumentResponse(
    Guid? Id,
    string? Title = null,
    string? Description = null,
    string? SkillFocus = null,
    DateTime? DueDate = null,
    bool? IsPublished = null,
    int? ExerciseCount = null,
    int? RecipientCount = null,
    DateTime? UpdatedAt = null);

public sealed record UserSearchPatchDocumentResponse(
    Guid? Id,
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    DateTime? LastLogin = null,
    int? CurrentLevel = null,
    DateTime? LastActivityAt = null,
    string? AvatarUrl = null,
    DateTime? UpdatedAt = null);

public sealed record UserSearchItemResponse(
	string Id,
	string Email,
	string Username,
	string? PhoneNumber,
	bool IsActive,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	int CurrentLevel);

public sealed record UserSearchResponse (
    string Id,
    string Email,
    string Username,
    string? PhoneNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CurrentLevel,
    string? AvatarUrl,
    DateTime? LastLogin
);