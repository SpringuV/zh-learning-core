namespace Lesson.Application.MediatR.Command.Course;

public record PublishCourseCommand(Guid CourseId) : IRequest<Result<PublishCourseResponseDTO>>;

public class PublishCourseHandler(ICourseRepository courseRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<PublishCourseHandler> logger) : IRequestHandler<PublishCourseCommand, Result<PublishCourseResponseDTO>>
{
    private readonly ILogger<PublishCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result<PublishCourseResponseDTO>> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate? courseAggregate = null;
            
            if (request.CourseId == Guid.Empty)
                return Result<PublishCourseResponseDTO>.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                courseAggregate = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
                if (courseAggregate is not null)
                {
                    courseAggregate.Publish();
                    await _courseRepository.UpdateAsync(courseAggregate, cancellationToken);

                    _logger.LogInformation("[PublishCourseHandler] Publishing {EventCount} domain events in-transaction", courseAggregate.DomainEvents.Count);
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

            return Result<PublishCourseResponseDTO>.SuccessResult(
                new PublishCourseResponseDTO(
                    CourseId: courseAggregate!.CourseId,
                    UpdatedAt: courseAggregate.UpdatedAt
                ),
                "Khóa học đã được xuất bản thành công."
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found with ID: {CourseId}", request.CourseId);
            return Result<PublishCourseResponseDTO>.FailureResult(
                "Không tìm thấy khóa học với ID: " + request.CourseId,
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Course with ID {CourseId} is already published.", request.CourseId);
            return Result<PublishCourseResponseDTO>.FailureResult(
                "Khóa học đã được xuất bản trước đó.",
                (int)ErrorCode.ALREADY_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing course with ID: {CourseId}", request.CourseId);
            return Result<PublishCourseResponseDTO>.FailureResult(
                "Đã xảy ra lỗi khi xuất bản khóa học.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}