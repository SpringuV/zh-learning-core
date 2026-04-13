using Lesson.Application.MediatR.Command.Topic;

namespace Lesson.Application.Services;

public class LessonService(IMediator mediator) : ILessonService
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task<Result<CreateCourseResponseDTO>> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new CreateCourseCommand(
            Title: request.Title,
            Description: request.Description,
            HskLevel: request.HskLevel,
            Slug: request.Slug
        ), cancellationToken);
    }

    public async Task<Result<CreateTopicResponseDTO>> CreateTopicAsync(TopicCreateRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new CreateTopicCommand(
            CourseId: request.CourseId,
            Title: request.Title,
            Description: request.Description,
            TopicType: request.TopicType,
            EstimatedTimeMinutes: request.EstimatedTimeMinutes,
            ExamYear: request.ExamYear,
            ExamCode: request.ExamCode
        ), cancellationToken);
    }

    public async Task<Result> ReorderCoursesAsync(CourseReorderRequestDTO request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}