using HanziAnhVu.Shared.Domain;

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

    public async Task<Result<CreateExerciseResponseDTO>> CreateExerciseAsync(ExerciseCreateRequestDTO request, CancellationToken cancellationToken)
    {
        // conver dto option => command option để gửi qua mediator, đảm bảo tách biệt giữa contract và domain, đồng thời tận dụng các logic validate và xử lý dữ liệu trong command handler.
        var listOptionCommand = request.Options?.Select(o => new ExerciseOption(
            id: o.Id,
            text: o.Text
        )).ToList(); // nếu request.Options là null thì listOptionCommand sẽ là null, nếu request.Options có giá trị thì sẽ convert từng phần tử sang ExerciseOption và tạo thành một List<ExerciseOption>
        // convert dto sang command để gửi qua mediator, đảm bảo tách biệt
        // giữa contract và domain, đồng thời tận dụng các logic validate 
        // và xử lý dữ liệu trong command handler.
        return await _mediator.Send(new CreateExerciseCommand(
            TopicId: request.TopicId,
            Description: request.Description,
            ExerciseType: request.ExerciseType,
            Difficulty: request.Difficulty,
            SkillType: request.SkillType,
            ExerciseContext: request.ExerciseContext,
            Question: request.Question,
            Options: listOptionCommand,
            CorrectAnswer: request.CorrectAnswer,
            Explanation: request.Explanation,
            AudioUrl: request.AudioUrl,
            ImageUrl: request.ImageUrl
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