namespace Search.Contracts.DTOs;

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
