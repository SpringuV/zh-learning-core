namespace Lesson.Application.MediatR.Command.Course;

using HanziAnhVu.Shared.Contracts.DTOs;
using HanziAnhVu.Shared.Contracts.Enums;

public record UpdateCourseCommand(
    Guid CourseId,
    string? Title,
    string? Description,
    int? OrderIndex,
    string? Slug
) : IRequest<Result<UpdateCourseResponseDTO>>;

public class UpdateCourseHandler(ICourseRepository courseRepository, IUnitOfWork unitOfWork, IPublisher publisher, ILogger<UpdateCourseHandler> logger) : IRequestHandler<UpdateCourseCommand, Result<UpdateCourseResponseDTO>>
{
    private readonly ILogger<UpdateCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result<UpdateCourseResponseDTO>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate? courseAggregate = null;
            
            if (request.CourseId == Guid.Empty)
                return Result<UpdateCourseResponseDTO>.FailureResult("CourseId không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                courseAggregate = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
                if (courseAggregate is not null)
                {
                    // Update fields if provided
                    if (request.Title is not null)
                        courseAggregate.UpdateTitle(request.Title);

                    if (request.Description is not null)
                        courseAggregate.UpdateDescription(request.Description);

                    if (request.OrderIndex.HasValue)
                        courseAggregate.UpdateOrderIndex(request.OrderIndex.Value);

                    await _courseRepository.UpdateAsync(courseAggregate, cancellationToken);
                }
                else
                    throw new KeyNotFoundException($"Không tìm thấy khóa học với ID: {request.CourseId}");
            }, cancellationToken);

            try
            {
                await _publisher.Publish(courseAggregate!.DomainEvents, cancellationToken);
                courseAggregate.PopDomainEvents();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish domain events for course {CourseId}", courseAggregate!.CourseId);
                // Continue - don't fail the operation
            }
            return Result<UpdateCourseResponseDTO>.SuccessResult(
                new UpdateCourseResponseDTO(
                    CourseId: courseAggregate!.CourseId,
                    Title: courseAggregate.Title,
                    Description: courseAggregate.Description,
                    OrderIndex: courseAggregate.OrderIndex,
                    Slug: courseAggregate.Slug,
                    UpdatedAt: courseAggregate.UpdatedAt
                ),
                "Khóa học đã được cập nhật thành công."
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found: {CourseId}", request.CourseId);
            return Result<UpdateCourseResponseDTO>.FailureResult(
                "Khóa học không tồn tại.",
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid course data");
            return Result<UpdateCourseResponseDTO>.FailureResult(
                "Dữ liệu khóa học không hợp lệ: " + ex.Message,
                (int)ErrorCode.VALIDATION
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating course {CourseId}", request.CourseId);
            return Result<UpdateCourseResponseDTO>.FailureResult(
                "Lỗi không mong muốn khi cập nhật khóa học.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}