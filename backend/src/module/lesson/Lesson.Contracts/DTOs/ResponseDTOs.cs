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
	string? AudioUrl,
	string? ImageUrl,
	IReadOnlyList<StartLearningExerciseOptionDTO> Options
);

public sealed record StartLearningResponseDTO(
	Guid SessionId,
	int TotalExercises,
	int CurrentSequenceNo,
	IReadOnlyList<StartLearningSessionItemDTO> SessionItems,
	StartLearningExerciseDTO FirstExercise
);
public sealed record CompleteLearningSessionResponseDTO(
    Guid SessionId,
    string SlugTopic,
    Guid UserId,
    float TotalScore,
    int TotalCorrect,
    int TotalWrong,
    int ScoreListening,
    int ScoreReading,
    int TimeSpentSeconds,
    DateTime CompletedAt
);
#endregion

#region Exercise DTOs
public sealed record SaveAnswerResponseDTO(
    Guid ExerciseId,
    Guid SessionId,
    DateTime AnsweredAt,
    string Status,
    int CurrentSequenceNo
);

public sealed record CreateExerciseResponseDTO(Guid ExerciseId);
#endregion