namespace Lesson.Application.MediatR.Command.Course;
public record UpdateCourseCommand(
    Guid CourseId,
    string? Title,
    string? Description,
    int? HskLevel
) : IRequest<Result>;

public class UpdateCourseHandler(ICourseRepository courseRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<UpdateCourseHandler> logger) : IRequestHandler<UpdateCourseCommand, Result>
{
    private readonly ILogger<UpdateCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate? courseAggregate = null;
            
            if (request.CourseId == Guid.Empty)
                return Result.FailureResult("CourseId không được để trống.", (int)ErrorCode.INVALID_ID);

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

                    if (request.HskLevel.HasValue)
                        courseAggregate.UpdateHskLevel(request.HskLevel.Value);

                    await _courseRepository.UpdateAsync(courseAggregate, cancellationToken);

                    _logger.LogInformation("[UpdateCourseHandler] Publishing {EventCount} domain events in-transaction", courseAggregate.DomainEvents.Count);
                    foreach (var domainEvent in courseAggregate.DomainEvents)
                    {
                        _logger.LogInformation("Publishing domain event {EventType} for course {CourseId}", domainEvent.GetType().Name, courseAggregate.CourseId);
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }
                    courseAggregate.PopDomainEvents();
                }
                else
                    throw new KeyNotFoundException($"Không tìm thấy khóa học với ID: {request.CourseId}");
            }, cancellationToken);

            return Result.SuccessResult("Khóa học đã được cập nhật thành công.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found: {CourseId}", request.CourseId);
            return Result.FailureResult(
                "Khóa học không tồn tại.",
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating course {CourseId}", request.CourseId);
            return Result.FailureResult(
                "Lỗi khi cập nhật khóa học.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}
