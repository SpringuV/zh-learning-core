using Lesson.Contracts.DTOs;

namespace Lesson.Contracts;

public interface ILessonService
{
    Task<CreateCourseResponseDTO> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken);
}