namespace Lesson.Application.MediatR.Command.Course;

public record CreateCourseCommand(
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    string Slug
) : IRequest<Result<CreateCourseResponseDTO>>;

public class CreateCourseHandler(ICourseRepository courseRepository, IUnitOfWork unitOfWork, IPublisher publisher, ILogger<CreateCourseHandler> logger) : IRequestHandler<CreateCourseCommand, Result<CreateCourseResponseDTO>>
{
    private readonly ILogger<CreateCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result<CreateCourseResponseDTO>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate courseAggregate = null!;
            
            // Phase 1: Create & Save
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                // Create aggregate 
                courseAggregate = CourseAggregate.CreateCourse(
                    request.Title, 
                    request.Description, 
                    request.HskLevel, 
                    request.OrderIndex, 
                    request.Slug
                );
                await _courseRepository.AddAsync(courseAggregate, cancellationToken);
            }, cancellationToken);
            
            // Phase 2: Publish events (best-effort)
            try
            {
                await _publisher.Publish(courseAggregate.DomainEvents, cancellationToken);
                courseAggregate.PopDomainEvents();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish domain events for course {CourseId}", courseAggregate.CourseId);
                // Continue - don't fail the operation
            }
            
            // Phase 3: Return success result
            return Result<CreateCourseResponseDTO>.SuccessResult(
                new CreateCourseResponseDTO(courseAggregate.CourseId),
                "Khóa học đã được tạo thành công."
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid course data");
            return Result<CreateCourseResponseDTO>.FailureResult(
                "Dữ liệu khóa học không hợp lệ: " + ex.Message,
                (int)ErrorCode.VALIDATION
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không mong muốn khi tạo khóa học");
            return Result<CreateCourseResponseDTO>.FailureResult(
                "Lỗi không mong muốn khi tạo khóa học.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}