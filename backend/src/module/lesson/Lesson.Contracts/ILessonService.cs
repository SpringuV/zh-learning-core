using HanziAnhVu.Shared.Contracts;
using Lesson.Contracts.DTOs;

namespace Lesson.Contracts;

public interface ILessonService
{
    #region Course
    Task<Result<CreateCourseResponseDTO>> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken);

    Task<Result> ReorderCoursesAsync(CourseReorderRequestDTO request, CancellationToken cancellationToken);
    #endregion
    
    #region Topic
    Task<Result<CreateTopicResponseDTO>> CreateTopicAsync(TopicCreateRequestDTO request, CancellationToken cancellationToken);
    #endregion
}