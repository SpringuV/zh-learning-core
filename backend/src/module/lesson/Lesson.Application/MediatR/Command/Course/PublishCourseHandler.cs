namespace Lesson.Application.MediatR.Command.Course;

public record PublishCourseCommand(Guid CourseId) : IRequest<Result>;

public class PublishCourseHandler(
        ICourseRepository courseRepository, 
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ITopicRepository topicRepository,
        ILogger<PublishCourseHandler> logger) 
    : IRequestHandler<PublishCourseCommand, Result>
{
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILogger<PublishCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate? courseAggregate = null;
            
            if (request.CourseId == Guid.Empty)
                return Result.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                courseAggregate = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
                // check course exists topic published or not, if not published any topic then course cannot be published, to avoid bad user experience when they enroll in the course but see no topic to learn.
                if (courseAggregate == null)
                {
                    _logger.LogWarning("Course with ID {CourseId} not found when publishing course.", request.CourseId);
                    throw new KeyNotFoundException($"Không tìm thấy khóa học với ID: {request.CourseId}");
                }
                var hasPublishedTopic = await _topicRepository.HasPublishedTopicByCourseIdAsync(request.CourseId, cancellationToken);
                if (!hasPublishedTopic)                {
                    _logger.LogWarning("Course with ID {CourseId} has no published topic when publishing course.", request.CourseId);
                    throw new InvalidOperationException("Không thể xuất bản khóa học khi chưa có chủ đề nào được xuất bản. Vui lòng xuất bản ít nhất một chủ đề trước khi xuất bản khóa học.");
                }
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

            return Result.SuccessResult(
                "Khóa học đã được xuất bản thành công."
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
                "Đã xảy ra lỗi khi xuất bản khóa học.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}