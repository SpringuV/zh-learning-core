namespace Lesson.Domain.Interface;

public record ExerciseWithAnswerDTORepository(
    Guid ExerciseId,
    string CorrectAnswer,
    SkillType SkillType,
    ExerciseType ExerciseType,
    ExerciseDifficulty Difficulty,
    IReadOnlyList<ExerciseOption> Options,
    string Question,
    string? Description,
    string? AudioUrl,
    string? ImageUrl,
    string? Explanation
);

public interface IExerciseRepository
{
    Task<bool> HasPublishedExerciseAsync(Guid topicId, CancellationToken ct = default);
    Task<IEnumerable<ExerciseAggregate>> GetByTopicIdAndPublishedAndOrderIndexAsync(Guid topicId, CancellationToken ct = default);
    Task<IEnumerable<ExerciseAggregate>> GetByTopicIdAndIdsAsync(Guid topicId, IEnumerable<Guid> ids, CancellationToken ct = default);
    Task ReorderByIdsAndTopicIdAsync(Guid topicId, IReadOnlyList<Guid> orderedExerciseIds, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<ExerciseAggregate> exercises, CancellationToken ct = default);
    Task<int?> GetMaxOrderIndexByTopicIdAsync(Guid topicId, CancellationToken ct = default);
    Task<ExerciseAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task UpdateAsync(ExerciseAggregate exercise, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ExerciseWithAnswerDTORepository>> GetExerciseWithAnswersAsync(IEnumerable<Guid> exerciseIds, CancellationToken ct = default);

}