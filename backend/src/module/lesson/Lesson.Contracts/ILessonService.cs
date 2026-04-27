using HanziAnhVu.Shared.Contracts;
using Lesson.Contracts.DTOs;

namespace Lesson.Contracts;

public interface ILessonService
{
    #region Course
    Task<Result> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<Result<CreateCourseResponseDTO>> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken);
    Task<Result> PublishCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<Result> UnPublishCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<Result> ReorderCoursesAsync(CourseReorderRequestDTO request, CancellationToken cancellationToken);
    Task<Result> UpdateCourseAsync(UpdateCourseRequestDTO request, CancellationToken cancellationToken);
    #endregion
    
    #region Topic
    Task<Result<SaveAnswerResponseDTO>> SaveAnswerAsync(SaveAnswerRequestDTO request, CancellationToken cancellationToken);
    Task<Result> DeleteTopicAsync(Guid topicId, CancellationToken cancellationToken);
    Task<Result> PublishTopicAsync(Guid topicId, CancellationToken cancellationToken);
    Task<Result> UnPublishTopicAsync(Guid topicId, CancellationToken cancellationToken);
    Task<Result> ReorderTopicsAsync(TopicReorderRequestDTO request, CancellationToken cancellationToken);
    Task<Result<CreateTopicResponseDTO>> CreateTopicAsync(TopicCreateRequestDTO request, CancellationToken cancellationToken);
    Task<Result> UpdateTopicAsync(UpdateTopicRequestDTO request, CancellationToken cancellationToken);
    Task<Result<StartLearningResponseDTO>> StartLearningAsync(StartLearningRequestDTO request, CancellationToken cancellationToken);
    Task<Result<CompleteLearningSessionResponseDTO>> CompleteLearningAsync(CompleteLearningSessionRequestDTO request, CancellationToken cancellationToken);
    #endregion

    #region Exercise
    Task<Result> DeleteExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);
    Task<Result> PublishExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);
    Task<Result> UnPublishExerciseAsync(Guid exerciseId, CancellationToken cancellationToken);
    Task<Result> ReorderExercisesAsync(ExerciseReorderRequestDTO request, CancellationToken cancellationToken);
    Task<Result<CreateExerciseResponseDTO>> CreateExerciseAsync(ExerciseCreateRequestDTO request, CancellationToken cancellationToken);
    Task<Result> UpdateExerciseAsync(UpdateExerciseRequestDTO request, CancellationToken cancellationToken);
    #endregion
}