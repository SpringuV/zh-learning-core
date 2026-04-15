using HanziAnhVu.Shared.Domain;
using HanziAnhVu.Shared.EventBus;
using Lesson.Domain.Entities.Exercise;

namespace Lesson.Contracts;

#region Course Events
public sealed record CourseCreatedIntegrationEvent(
    Guid CourseId,
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    string Slug,
    long TotalStudentsEnrolled,
    long TotalTopics,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseTitleUpdatedIntegrationEvent(
    Guid CourseId,
    string NewTitle,
    string NewSlug,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseDescriptionUpdatedIntegrationEvent(
    Guid CourseId,
    string NewDescription,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseHskLevelUpdatedIntegrationEvent(
    Guid CourseId,
    int NewHskLevel,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseReOrderedIntegrationEvent(
    List<Guid> OrderedCourseIds,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CoursePublishedIntegrationEvent(
    Guid CourseId,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseUnpublishedIntegrationEvent(
    Guid CourseId,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicAddedToCourseIntegrationEvent(
    Guid CourseId,
    Guid TopicId,
    int OrderIndex,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseEnrollmentCountUpdatedIntegrationEvent(
    Guid CourseId,
    long TotalStudentsEnrolled,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record CourseTotalTopicsUpdatedIntegrationEvent(
    Guid CourseId,
    long TotalTopics,
    DateTime UpdatedAt
): IntegrationEvent;
#endregion

#region Topic Events
public sealed record TopicCreatedIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    string Title,
    string Description,
    string Slug,
    string TopicType,
    int EstimatedTimeMinutes,
    int ExamYear,
    string ExamCode,
    int OrderIndex,
    bool IsPublished,
    long TotalExercises,
    DateTime CreatedAt,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicTitleUpdatedIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    string NewTitle,
    string NewSlug,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicDescriptionUpdatedIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    string NewDescription,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicEstimatedTimeUpdatedIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    int NewEstimatedTimeMinutes,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicReOrderedIntegrationEvent(
    Guid CourseId,
    IReadOnlyList<Guid> OrderedTopicIds,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicExamInfoUpdatedIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    int NewExamYear,
    string NewExamCode,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record TopicPublishedIntegrationEvent(
    Guid TopicId,
    DateTime PublishedAt
): IntegrationEvent;

public sealed record TopicUnpublishedIntegrationEvent(
    Guid TopicId,
    DateTime UnpublishedAt
): IntegrationEvent;

public sealed record ExerciseAddedToTopicIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    Guid ExerciseId,
    DateTime AddedAt
): IntegrationEvent;

public sealed record ExerciseRemovedFromTopicIntegrationEvent(
    Guid TopicId,
    Guid CourseId,
    Guid ExerciseId,
    DateTime RemovedAt
): IntegrationEvent;

public sealed record TopicTotalExercisesUpdatedIntegrationEvent(
    Guid TopicId,
    long TotalExercises,
    DateTime UpdatedAt
): IntegrationEvent;
#endregion

#region Exercise Events
public sealed record ExerciseCreatedIntegrationEvent(
    Guid ExerciseId,
    Guid TopicId,
    string Description,
    int OrderIndex,
    ExerciseType ExerciseType,
    SkillType SkillType,
    string Question,
    string CorrectAnswer,
    ExerciseDifficulty Difficulty,
    ExerciseContext Context,
    string AudioUrl,
    string ImageUrl,
    string Explanation,
    string Slug,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ExerciseOption> Options
): IntegrationEvent;

public sealed record ExerciseDescriptionUpdatedIntegrationEvent(
    Guid ExerciseId,
    string NewDescription,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseReOrderedIntegrationEvent(
    Guid TopicId,
    IReadOnlyList<Guid> OrderedExerciseIds,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseQuestionUpdatedIntegrationEvent(
    Guid ExerciseId,
    string NewQuestion,
    string NewSlug,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseTypeUpdatedIntegrationEvent(
    Guid ExerciseId,
    Guid TopicId,
    ExerciseType NewExerciseType,
    string NewSlug,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseSkillTypeUpdatedIntegrationEvent(
    Guid ExerciseId,
    Guid TopicId,
    SkillType NewSkillType,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseCorrectAnswerUpdatedIntegrationEvent(
    Guid ExerciseId,
    string NewCorrectAnswer,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseDifficultyUpdatedIntegrationEvent(
    Guid ExerciseId,
    ExerciseDifficulty NewDifficulty,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseContextUpdatedIntegrationEvent(
    Guid ExerciseId,
    Guid TopicId,
    ExerciseContext NewContext,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseAudioUrlUpdatedIntegrationEvent(
    Guid ExerciseId,
    string NewAudioUrl,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseImageUrlUpdatedIntegrationEvent(
    Guid ExerciseId,
    string NewImageUrl,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExerciseExplanationUpdatedIntegrationEvent(
    Guid ExerciseId,
    Guid TopicId,
    string NewExplanation,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record ExercisePublishedIntegrationEvent(
    Guid ExerciseId,
    DateTime PublishedAt
): IntegrationEvent;

public sealed record ExerciseUnpublishedIntegrationEvent(
    Guid ExerciseId,
    DateTime UnpublishedAt
): IntegrationEvent;

public sealed record ExerciseOptionsUpdatedIntegrationEvent(
    Guid ExerciseId,
    List<ExerciseOption> NewOptions,
    DateTime UpdatedAt
): IntegrationEvent;
#endregion

#region Session Exercise Events
public sealed record UserTopicExerciseSessionStartedIntegrationEvent(
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    DateTime StartedAt
): IntegrationEvent;

public sealed record UserTopicExerciseSessionCompletedIntegrationEvent(
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    float SessionScore,
    int TotalAttempts,
    int TimeSpentSeconds,
    DateTime CompletedAt
): IntegrationEvent;

public sealed record UserTopicExerciseSessionAbandonedIntegrationEvent(
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    DateTime AbandonedAt
): IntegrationEvent;
#endregion

#region Attempt Exercise Events
public sealed record ExerciseAttemptCreatedIntegrationEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid? TopicId,
    Guid ExerciseId,
    string Question,
    ExerciseType ExerciseType,
    SkillType SkillType,
    ExerciseDifficulty Difficulty,
    string Answer,
    float InitialScore,
    bool IsCorrect,
    DateTime CreatedAt
): IntegrationEvent;

public sealed record ExerciseAttemptScoredIntegrationEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid ExerciseId,
    SkillType SkillType,
    float Score,
    bool IsCorrect,
    DateTime ScoredAt
): IntegrationEvent;

public sealed record ExerciseAttemptAiFeedbackAddedIntegrationEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid ExerciseId,
    string Feedback,
    DateTime FeedbackAddedAt
): IntegrationEvent;

public sealed record ExerciseAttemptAnswerChangedIntegrationEvent(
    Guid AttemptId,
    Guid SessionId,
    Guid UserId,
    Guid ExerciseId,
    string NewAnswer,
    DateTime ChangedAt
): IntegrationEvent;
#endregion

#region User Progress Events
public sealed record UserTopicProgressCreatedIntegrationEvent(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    int TotalAttempts,
    int TotalAnswered,
    int TotalCorrect,
    int TotalWrong,
    float TotalScore,
    float? AccuracyRate,
    DateTime CreatedAt
): IntegrationEvent;

public sealed record UserTopicProgressUpdatedIntegrationEvent(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    int? TotalAttempts,
    int? TotalAnswered,
    int? TotalCorrect,
    int? TotalWrong,
    float? TotalScore,
    float? AccuracyRate,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record UserTopicProgressResetIntegrationEvent(
    Guid TopicProgressId,
    Guid UserId,
    Guid TopicId,
    DateTime ResetAt
): IntegrationEvent;
#endregion

#region Flashcard Events
public sealed record FlashcardCreatedIntegrationEvent(
    Guid FlashcardId,
    Guid CourseId,
    Guid TopicId,
    string FrontTextChinese,
    string Pinyin,
    string MeaningVi,
    string? MeaningEn,
    string PhraseType,
    int OrderIndex,
    string? AudioUrl,
    int? HskLevel,
    bool IsHskCore,
    string? ExampleSentenceChinese,
    string? ExampleSentencePinyin,
    string? ExampleSentenceMeaningVi,
    string? Radical,
    int? StrokeCount,
    string? TraditionalForm,
    DateTime CreatedAt
): IntegrationEvent;

public sealed record FlashcardTextUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    string FrontTextChinese,
    string Pinyin,
    string MeaningVi,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardEnglishMeaningUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    string MeaningEn,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardPhraseTypeUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    string PhraseType,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardOrderIndexUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    int OrderIndex,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardAudioUrlUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    string AudioUrl,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardHskInfoUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    int? HskLevel,
    bool IsHskCore,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardExampleSentenceUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    string? ExampleSentenceChinese,
    string? ExampleSentencePinyin,
    string? ExampleSentenceMeaningVi,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardCharacterInfoUpdatedIntegrationEvent(
    Guid FlashcardId,
    Guid TopicId,
    string? Radical,
    int? StrokeCount,
    string? TraditionalForm,
    DateTime UpdatedAt
): IntegrationEvent;

public sealed record FlashcardDeletedIntegrationEvent(
    Guid FlashcardId,
    Guid CourseId,
    Guid TopicId,
    DateTime DeletedAt
): IntegrationEvent;
#endregion