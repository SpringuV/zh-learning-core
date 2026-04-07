namespace Search.Contracts.DTOs;

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
