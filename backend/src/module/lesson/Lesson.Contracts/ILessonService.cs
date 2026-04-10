using HanziAnhVu.Shared.Contracts;
using Lesson.Contracts.DTOs;

namespace Lesson.Contracts;

public interface ILessonService
{
    Task<Result<CreateCourseResponseDTO>> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken);
}