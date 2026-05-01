using HanziAnhVu.Shared.Contracts;
using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface IExerciseSearchQueriesService
{
    Task BulkIndexExerciseAttemptAsync(ExerciseAttemptBatchScoredRequestDTO request, CancellationToken cancellationToken = default);
    Task CompletedExerciseSessionAsync(ExerciseSessionCompletedRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<LearningExerciseSessionPracticeDTOResponse>> GetExerciseSessionPracticeItemWithoutAnswerAsync(Guid exerciseId, Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<CountinueLearningResponseDTO>> CountinueExercisesSessionForTopicClientAsync(string slug, Guid userId, CancellationToken cancellationToken = default);
    Task<ExerciseSearchDetailResponse> GetExerciseDetailSearchItemAdminAsync(Guid exerciseId, CancellationToken cancellationToken = default);
    Task<ExerciseSearchWithTopicMetadataResponse> SearchExerciseAdminWithTopicMetadataAsync(ExerciseSearchQueryRequest request, CancellationToken cancellationToken = default);
    Task UpdateOptionsAsync(ExerciseOptionsUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateExplanationAsync(ExerciseExplanationUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateImageUrlAsync(ExerciseImageUrlUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateAudioUrlAsync(ExerciseAudioUrlUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateDifficultyAsync(ExerciseDifficultyUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateCorrectAnswerAsync(ExerciseCorrectAnswerUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateQuestionAsync(ExerciseQuestionUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateDescriptionAsync(ExerciseDescriptionUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateExerciseContextAsync(ExerciseContextUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateExerciseSkillTypeAsync(ExerciseSkillTypeUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task UpdateExerciseTypeAsync(ExerciseTypeUpdatedRequestDTO request, CancellationToken cancellationToken = default);
    Task<ExerciseIndexResponse> IndexAsync(ExerciseSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExerciseDeletedRequestDTO request, CancellationToken cancellationToken = default);
    Task PublishAsync(ExercisePublishedRequestDTO request, CancellationToken cancellationToken = default);
    Task UnPublishAsync(ExerciseUnPublishedRequestDTO request, CancellationToken cancellationToken = default);
    Task ReOrderAsync(ExerciseReorderSearchRequestDTO request, CancellationToken cancellationToken = default);
    Task<SearchQueryResult<ExerciseSearchItemAdminResponse>> SearchExerciseItemAdminAsync(ExerciseSearchQueryRequest request, CancellationToken cancellationToken = default);
}