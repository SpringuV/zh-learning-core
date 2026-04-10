namespace Lesson.Contracts.DTOs;
public record BaseResponseDTO(bool Success, string Message);
public sealed record CreateCourseResponseDTO(Guid? CourseId = null): BaseResponseDTO(Success: CourseId.HasValue, Message: CourseId.HasValue ? "Khóa học đã được tạo thành công." : "Không thể tạo khóa học.");