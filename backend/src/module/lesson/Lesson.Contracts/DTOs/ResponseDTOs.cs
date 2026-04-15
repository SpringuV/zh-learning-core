namespace Lesson.Contracts.DTOs;

#region Course DTOs
public sealed record CreateCourseResponseDTO(Guid CourseId);

#endregion

#region Topic DTOs
public sealed record CreateTopicResponseDTO(Guid TopicId);

#endregion

#region Exercise DTOs
public sealed record CreateExerciseResponseDTO(Guid ExerciseId);
#endregion