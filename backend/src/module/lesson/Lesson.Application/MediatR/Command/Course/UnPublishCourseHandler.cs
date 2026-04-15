namespace Lesson.Application.MediatR.Command.Course;

public record UnPublishCourseCommand(Guid CourseId) : IRequest<Result>;
public class UnPublishCourseHandler(ICourseRepository courseRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<UnPublishCourseHandler> logger) : IRequestHandler<UnPublishCourseCommand, Result>
{
    private readonly ILogger<UnPublishCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(UnPublishCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate? courseAggregate = null;
            
            if (request.CourseId == Guid.Empty)
                return Result.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                courseAggregate = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
                if (courseAggregate is not null)
                {
                    courseAggregate.UnPublish();
                    await _courseRepository.UpdateAsync(courseAggregate, cancellationToken);

                    _logger.LogInformation("[UnPublishCourseHandler] Publishing {EventCount} domain events in-transaction", courseAggregate.DomainEvents.Count);
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

            return Result.SuccessResult(
                "Khóa học đã được hủy xuất bản thành công."
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found with ID: {CourseId}", request.CourseId);
            return Result.FailureResult(
                "Không tìm thấy khóa học với ID: " + request.CourseId,
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Course with ID {CourseId} is already published.", request.CourseId);
            return Result.FailureResult(
                "Khóa học đã được xuất bản trước đó.",
                (int)ErrorCode.ALREADY_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing course with ID: {CourseId}", request.CourseId);
            return Result.FailureResult(
                "Đã xảy ra lỗi khi hủy xuất bản khóa học.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}