namespace Lesson.Application.Services;

public class LessonService(IMediator mediator) : ILessonService
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public Task<Result<CreateCourseResponseDTO>> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken)
    {
        return _mediator.Send(new CreateCourseCommand(
            Title: request.Title,
            Description: request.Description,
            HskLevel: request.HskLevel,
            Slug: request.Slug
        ), cancellationToken);
    }

    public Task<Result> ReorderCoursesAsync(CourseReorderRequestDTO request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}