namespace Lesson.Application.Services;

public class LessonService(IMediator mediator) : ILessonService
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task<Result<CreateCourseResponseDTO>> CreateCourseAsync(CreateCourseRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new CreateCourseCommand(
            Title: request.Title,
            Description: request.Description,
            HskLevel: request.HskLevel
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

    public async Task<Result> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DeleteCourseCommand(CourseId: courseId), cancellationToken);
    }

    public async Task<Result> DeleteExerciseAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DeleteExerciseCommand(ExerciseId: exerciseId), cancellationToken);
    }

    public async Task<Result> DeleteTopicAsync(Guid topicId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new DeleteTopicCommand(TopicId: topicId), cancellationToken);
    }

    public async Task<Result> PublishCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new PublishCourseCommand(
            CourseId: courseId
        ), cancellationToken);
    }

    public async Task<Result> PublishExerciseAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new PublishExerciseCommand(
            ExerciseId: exerciseId
        ), cancellationToken);
    }

    public async Task<Result> PublishTopicAsync(Guid topicId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new PublishTopicCommand(
            TopicId: topicId
        ), cancellationToken);
    }

    public async Task<Result> ReorderCoursesAsync(CourseReorderRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ReOrderCourseCommand(
            OrderedCourseIds: request.OrderedCourseIds
        ), cancellationToken);
    }

    public async Task<Result> ReorderExercisesAsync(ExerciseReorderRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ReOrderExerciseCommand(
            OrderedExerciseIds: request.OrderedExerciseIds,
            TopicId: request.TopicId
        ), cancellationToken);
    }

    public async Task<Result> ReorderTopicsAsync(TopicReorderRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ReOrderTopicCommand(
            OrderedTopicIds: request.OrderedTopicIds,
            CourseId: request.CourseId
        ), cancellationToken);
    }

    public async Task<Result> UnPublishCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new UnPublishCourseCommand(
            CourseId: courseId
        ), cancellationToken);
    }

    public async Task<Result> UnPublishExerciseAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new UnPublishExerciseCommand(
            ExerciseId: exerciseId
        ), cancellationToken);
    }

    public async Task<Result> UnPublishTopicAsync(Guid topicId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new UnPublishTopicCommand(
            TopicId: topicId
        ), cancellationToken);
    }

    public async Task<Result> UpdateCourseAsync(UpdateCourseRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new UpdateCourseCommand(
            CourseId: request.CourseId,
            Title: request.Title,
            Description: request.Description,
            HskLevel: request.HskLevel
        ), cancellationToken);
    }

    public async Task<Result> UpdateExerciseAsync(UpdateExerciseRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new UpdateExerciseCommand(
            ExerciseId: request.ExerciseId,
            Description: request.Description,
            Question: request.Question,
            CorrectAnswer: request.CorrectAnswer,
            ExerciseType: request.ExerciseType,
            SkillType: request.SkillType,
            Difficulty: request.Difficulty,
            ExerciseContext: request.ExerciseContext,
            AudioUrl: request.AudioUrl,
            ImageUrl: request.ImageUrl,
            Explanation: request.Explanation,
            Options: request.Options
        ), cancellationToken);
    }

    public async Task<Result> UpdateTopicAsync(UpdateTopicRequestDTO request, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new UpdateTopicCommand(
            TopicId: request.TopicId,
            Title: request.Title,
            Description: request.Description,
            EstimatedTimeMinutes: request.EstimatedTimeMinutes,
            NewExamYear: request.NewExamYear,
            NewExamCode: request.NewExamCode
        ), cancellationToken);
    }
}