namespace Lesson.Application.MediatR.Command.Topic;

public record StartLearningCommand(Guid UserId, string SlugTopic) : IRequest<Result<StartLearningResponseDTO>>;

// validate input, load topic + published exercises, create UserTopicExerciseSessionAggregate + TopicProgressAggregate (if not exist) trong 1 transaction, publish domain events, return success.
public class ValidatorStartLearning : AbstractValidator<StartLearningCommand>
{
    public ValidatorStartLearning()
    {
        RuleFor(cmd => cmd.UserId).NotEmpty().WithMessage("UserId không được để trống.");
        RuleFor(cmd => cmd.SlugTopic).NotEmpty().WithMessage("SlugTopic không được để trống.");
    }
}

public class StartLearningHandler(ITopicProgressRepository topicProgressRepository,
    IPublisher publisher,
    ILessonUnitOfWork unitOfWork,
    ICourseRepository courseRepository,
    ITopicRepository topicRepository,
    IExerciseRepository exerciseRepository,
    IUserTopicExerciseSessionRepository userTopicExerciseSessionRepository,
    ILogger<StartLearningHandler> logger) : IRequestHandler<StartLearningCommand, Result<StartLearningResponseDTO>>
{
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILogger<StartLearningHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUserTopicExerciseSessionRepository _userTopicExerciseSessionRepository = userTopicExerciseSessionRepository ?? throw new ArgumentNullException(nameof(userTopicExerciseSessionRepository));
    private readonly ITopicProgressRepository _topicProgressRepository = topicProgressRepository ?? throw new ArgumentNullException(nameof(topicProgressRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    public async Task<Result<StartLearningResponseDTO>> Handle(StartLearningCommand request, CancellationToken cancellationToken)
    {
        var topicAggregate = null as TopicAggregate;
        try
        {
            topicAggregate = await _topicRepository.GetBySlugAsync(request.SlugTopic, cancellationToken);
            if (topicAggregate == null)
            {
                _logger.LogWarning("Topic with slug {SlugTopic} not found when starting learning. UserId: {UserId}", request.SlugTopic, request.UserId);
                return Result<StartLearningResponseDTO>.FailureResult("Không tìm thấy chủ đề học.", (int)ErrorCode.NOTFOUND);
            }

            // load tất cả bài tập đã xuất bản của chủ đề, sắp xếp theo OrderIndex và ExerciseId để đảm bảo thứ tự ổn định.
            var publishedExercises = await _exerciseRepository.GetByTopicIdAndPublishedAndOrderIndexAsync(topicAggregate.TopicId, cancellationToken);
            // load hsk from course
            var hskLevel = await _courseRepository.GetHskLevelByCourseIdAsync(topicAggregate.CourseId, cancellationToken);
            if (publishedExercises.Count() == 0)
            {
                _logger.LogWarning("Topic {SlugTopic} has no published exercises when starting learning. UserId: {UserId}", request.SlugTopic, request.UserId);
                return Result<StartLearningResponseDTO>.FailureResult("Chủ đề chưa có bài tập đã xuất bản.", (int)ErrorCode.NOTFOUND);
            }

            UserTopicExerciseSessionAggregate userTopicExerciseSessionAggregate = null!;
            TopicProgressAggregate topicProgressAggregate = null!;

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                userTopicExerciseSessionAggregate = UserTopicExerciseSessionAggregate.Create(request.UserId, topicAggregate.TopicId, hskLevel);
                var sessionItems = publishedExercises.Select((exercise, index) =>
                    UserTopicExerciseSessionItem.Create(
                        userTopicExerciseSessionAggregate.SessionId,
                        exercise.ExerciseId,
                        index + 1,
                        exercise.OrderIndex)).ToList();
                userTopicExerciseSessionAggregate.SetSessionItems(sessionItems);

                // Kiểm tra nếu đã có topic progress thì không tạo mới, tránh mất lịch sử cũ. Nếu chưa có thì tạo mới.
                var existingTopicProgress = await _topicProgressRepository.GetByUserIdAndTopicIdAsync(request.UserId, topicAggregate.TopicId, cancellationToken);
                topicProgressAggregate = existingTopicProgress ?? TopicProgressAggregate.Create(request.UserId, topicAggregate.TopicId);

                await _userTopicExerciseSessionRepository.AddAsync(userTopicExerciseSessionAggregate, cancellationToken);

                // Nếu chưa có progress thì mới thêm mới, nếu đã có rồi thì chỉ update (vì có thể có domain event mới từ việc tạo session).
                if (existingTopicProgress is null)
                {
                    await _topicProgressRepository.AddAsync(topicProgressAggregate, cancellationToken);
                }

                foreach (var domainEvent in topicProgressAggregate.DomainEvents)
                {
                    _logger.LogInformation("Publishing {EventType} for topic progress. UserId: {UserId}, TopicId: {TopicId}", domainEvent.GetType().Name, request.UserId, topicAggregate.TopicId);
                    await _publisher.Publish(domainEvent, cancellationToken);
                }

                foreach (var domainEvent in userTopicExerciseSessionAggregate.DomainEvents)
                {
                    _logger.LogInformation("Publishing {EventType} for new exercise session. UserId: {UserId}, TopicId: {TopicId}", domainEvent.GetType().Name, request.UserId, topicAggregate.TopicId);
                    await _publisher.Publish(domainEvent, cancellationToken);
                }

                topicProgressAggregate.PopDomainEvents();
                userTopicExerciseSessionAggregate.PopDomainEvents();
            }, cancellationToken);

            var response = BuildStartLearningResponse(userTopicExerciseSessionAggregate, topicProgressAggregate, [.. publishedExercises]);
            return Result<StartLearningResponseDTO>.SuccessResult(response, message: "Bắt đầu học chủ đề thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when starting learning for Topic with Slug {SlugTopic} and UserId {UserId}. Details: {Message}", request.SlugTopic, request.UserId, ex.Message);
            return Result<StartLearningResponseDTO>.FailureResult("Đã xảy ra lỗi không mong muốn khi bắt đầu học. Vui lòng thử lại sau.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }

    private static StartLearningResponseDTO BuildStartLearningResponse(
        UserTopicExerciseSessionAggregate sessionAggregate,
        TopicProgressAggregate topicProgressAggregate,
        IReadOnlyList<ExerciseAggregate> publishedExercises)
    {
        ArgumentNullException.ThrowIfNull(sessionAggregate);
        ArgumentNullException.ThrowIfNull(topicProgressAggregate);
        ArgumentNullException.ThrowIfNull(publishedExercises);

        var sessionItems = sessionAggregate.SessionItems
            .Select(item => new StartLearningSessionItemDTO(
                SessionItemId: item.SessionItemId,
                ExerciseId: item.ExerciseId,
                SequenceNo: item.SequenceNo,
                OrderIndex: item.OrderIndex,
                AttemptId: item.AttemptId,
                Status: item.Status.ToString(),
                ViewedAt: item.ViewedAt,
                AnsweredAt: item.AnsweredAt))
            .ToList();

        var firstExercise = publishedExercises[0];
        var firstExerciseDto = new StartLearningExerciseDTO(
            ExerciseId: firstExercise.ExerciseId,
            TopicId: firstExercise.TopicId,
            OrderIndex: firstExercise.OrderIndex,
            Description: firstExercise.Description,
            Question: firstExercise.Question,
            ExerciseType: firstExercise.ExerciseType.ToString(),
            SkillType: firstExercise.SkillType.ToString(),
            Difficulty: firstExercise.Difficulty.ToString(),
            AudioUrl: firstExercise.AudioUrl,
            ImageUrl: firstExercise.ImageUrl,
            Options: [.. firstExercise.Options
                .Select(option => new StartLearningExerciseOptionDTO(
                    Id: option.Id,
                    Text: option.Text))]);

        return new StartLearningResponseDTO(
            SessionId: sessionAggregate.SessionId,
            TotalExercises: sessionAggregate.TotalExercises,
            CurrentSequenceNo: sessionAggregate.CurrentSequenceNo,
            SessionItems: sessionItems,
            FirstExercise: firstExerciseDto
        );
    }
}