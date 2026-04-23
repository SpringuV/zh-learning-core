namespace Lesson.Contracts.DTOs;

#region Course DTOs
public sealed record CreateCourseResponseDTO(Guid CourseId);

#endregion

#region Topic DTOs
public sealed record CreateTopicResponseDTO(Guid TopicId);

public sealed record StartLearningSessionItemDTO(
	Guid SessionItemId,
	Guid ExerciseId,
	int SequenceNo,
	int OrderIndex,
	Guid? AttemptId,
	string Status,
	DateTime? ViewedAt,
	DateTime? AnsweredAt
);

public sealed record StartLearningExerciseOptionDTO(
	string Id,
	string Text
);

public sealed record StartLearningExerciseDTO(
	Guid ExerciseId,
	Guid TopicId,
	int OrderIndex,
	string Description,
	string Question,
	string ExerciseType,
	string SkillType,
	string Difficulty,
	string Context,
	string AudioUrl,
	string ImageUrl,
	IReadOnlyList<StartLearningExerciseOptionDTO> Options
);

public sealed record StartLearningResponseDTO(
	Guid SessionId,
	Guid TopicProgressId,
	int TotalExercises,
	int CurrentSequenceNo,
	IReadOnlyList<StartLearningSessionItemDTO> SessionItems,
	StartLearningExerciseDTO FirstExercise
);

#endregion

#region Exercise DTOs
public sealed record CreateExerciseResponseDTO(Guid ExerciseId);
#endregion