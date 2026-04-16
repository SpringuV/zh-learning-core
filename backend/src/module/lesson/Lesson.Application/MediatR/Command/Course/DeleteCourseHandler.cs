namespace Lesson.Application.MediatR.Command.Course;

public record DeleteCourseCommand(Guid CourseId) : IRequest<Result>;

public class DeleteCourseCommandHandler(
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        ICourseRepository courseRepository,
        ILogger<DeleteCourseCommandHandler> logger) 
    : IRequestHandler<DeleteCourseCommand, Result>
{
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILogger<DeleteCourseCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                var courseAggregate = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken) ?? throw new KeyNotFoundException($"Course with id '{request.CourseId}' was not found.");
                
                courseAggregate.Delete();
                
                await _courseRepository.DeleteAsync(courseAggregate.CourseId, cancellationToken);

                // Publish domain events
                var domainEvents = courseAggregate.DomainEvents;
                foreach (var domainEvent in domainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                courseAggregate.PopDomainEvents();
            }, cancellationToken);

            return Result.SuccessResult();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found with ID: {CourseId}", request.CourseId);
            return Result.FailureResult(
                ex.Message,
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Course with ID {CourseId} cannot be deleted. Reason: {Reason}", request.CourseId, ex.Message);
            return Result.FailureResult(
                ex.Message,
                (int)ErrorCode.INVALID_OPERATION
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during course delete operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                ex.Message,
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}