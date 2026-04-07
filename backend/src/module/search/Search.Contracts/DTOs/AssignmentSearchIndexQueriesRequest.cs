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
