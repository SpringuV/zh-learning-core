namespace Lesson.Application.MediatR.Command.Course;

public record CreateCourseCommand(
    string Title,
    string Description,
    int HskLevel,
    int OrderIndex,
    string Slug
) : IRequest<CreateCourseResponseDTO>;

public class CreateCourseHandler(ICourseRepository courseRepository, IUnitOfWork unitOfWork, IPublisher publisher, ILogger<CreateCourseHandler> logger) : IRequestHandler<CreateCourseCommand, CreateCourseResponseDTO>
{
    private readonly ILogger<CreateCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    public async Task<CreateCourseResponseDTO> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            CourseAggregate courseAggregate = null!;
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                // create aggregate 
                courseAggregate = CourseAggregate.CreateCourse(
                    request.Title, 
                    request.Description, 
                    request.HskLevel, 
                    request.OrderIndex, 
                    request.Slug
                );
                await _courseRepository.AddAsync(
                    courseAggregate,
                    cancellationToken
                );
            }, cancellationToken);
            try
            {
                await _publisher.Publish(courseAggregate.DomainEvents, cancellationToken);
                courseAggregate.PopDomainEvents(); // Clear events after publishing
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish domain events for course {CourseId}", courseAggregate.CourseId);
                // Optionally, you could choose to rethrow or handle the failure according to your needs
            }
            return new CreateCourseResponseDTO(courseAggregate.CourseId);
        }
        catch (ArgumentException ex)
        {
            // Log hoặc ném domain-specific exception
            throw new InvalidOperationException("Dữ liệu khóa học không hợp lệ", ex);
        }
        catch (Exception ex) // Unexpected errors
        {
            // Log critical
            _logger.LogError(ex, "Lỗi không mong muốn khi tạo khóa học");
            throw;
        }
    }
}