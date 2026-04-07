namespace Classroom.Contracts.DTOs;

/// <summary>
/// DTO for assignment search results (denormalized, read-optimized)
/// Used in CQRS Read side (Elasticsearch)
/// </summary>
public class AssignmentSearchResultDTOs
{
    public Guid AssignmentId { get; set; }
    public Guid ClassroomId { get; set; }
    public Guid TeacherId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AssignmentType { get; set; } = string.Empty;  // AllClass, Individual
    public string SkillFocus { get; set; } = string.Empty;      // Reading, Writing, Listening, Speaking
    public DateTime DueDate { get; set; }
    public bool IsPublished { get; set; }
    public int ExerciseCount { get; set; }
    public int RecipientCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
