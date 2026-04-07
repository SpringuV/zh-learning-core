namespace Search.Contracts.DTOs;

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
