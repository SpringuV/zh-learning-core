namespace Lesson.Application.MediatR.Command.Course;

public record CreateCourseCommand(
    string Title,
    string Description,
    int HskLevel
) : IRequest<Result<CreateCourseResponseDTO>>;

public class CreateCourseHandler(
    ICourseRepository courseRepository, 
    ILessonUnitOfWork unitOfWork, 
    IPublisher publisher, 
    ILogger<CreateCourseHandler> logger) : IRequestHandler<CreateCourseCommand, Result<CreateCourseResponseDTO>>
{
    private readonly ILogger<CreateCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result<CreateCourseResponseDTO>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate courseAggregate = null!;
            _logger.LogInformation("[CreateCourseHandler] Starting course creation - Title: {Title}, HskLevel: {HskLevel}", 
                request.Title, request.HskLevel);

            var maxOrder = await _courseRepository.GetMaxOrderIndexAsync(cancellationToken);
            _logger.LogInformation("[CreateCourseHandler] Current max order index: {MaxOrderIndex}", maxOrder);

            // Create aggregate + publish domain events (write outbox) + save DB in one transaction
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                courseAggregate = CourseAggregate.CreateCourse(
                    request.Title,
                    request.Description,
                    request.HskLevel,
                    (maxOrder ?? 0) + 1
                );

                await _courseRepository.AddAsync(courseAggregate, cancellationToken);
                _logger.LogInformation("[CreateCourseHandler] Aggregate added to repository");

                foreach (var domainEvent in courseAggregate.DomainEvents)
                {
                    _logger.LogInformation("[CreateCourseHandler] Publishing {EventType} for {AggregateId}", domainEvent.GetType().Name, courseAggregate.CourseId);
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                courseAggregate.PopDomainEvents();
            }, cancellationToken);
                    
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict while creating course");
            return Result<CreateCourseResponseDTO>.FailureResult(
                ex.Message,
                (int)ErrorCode.DUPLICATE
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