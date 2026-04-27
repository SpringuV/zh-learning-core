namespace Lesson.Application.MediatR.Command.Topic;

public sealed record SubmitCompleteLearningSessionCommand(
    Guid SessionId,
    string SlugTopic,
    Guid UserId
) : IRequest<Result<CompleteLearningSessionResponseDTO>>;

// phần writing sẽ được chấm riêng, và có API riêng
public sealed class SubmitCompleteLearningSessionCommandHandler(
    IExerciseRepository exerciseRepository,
    IExerciseAttemptRepository exerciseAttemptRepository, 
    ITopicRepository topicRepository, 
    IUserTopicExerciseSessionRepository userTopicExerciseSessionRepository, 
    ILessonUnitOfWork unitOfWork, 
    ILogger<SubmitCompleteLearningSessionCommandHandler> logger, IPublisher publisher) 
    : IRequestHandler<SubmitCompleteLearningSessionCommand, Result<CompleteLearningSessionResponseDTO>>
{
    private readonly IExerciseRepository exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly IExerciseAttemptRepository _exerciseAttemptRepository = exerciseAttemptRepository ?? throw new ArgumentNullException(nameof(exerciseAttemptRepository));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly IUserTopicExerciseSessionRepository _userTopicExerciseSessionRepository = userTopicExerciseSessionRepository ?? throw new ArgumentNullException(nameof(userTopicExerciseSessionRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<SubmitCompleteLearningSessionCommandHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<CompleteLearningSessionResponseDTO>> Handle(SubmitCompleteLearningSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionAggregate = null as UserTopicExerciseSessionAggregate;
            var topicAggregate = null as TopicAggregate;
            if (request.SessionId == Guid.Empty)
            {
                // fallback tìm bằng slug topic và user id, vì có thể client sẽ không có session id mà chỉ có slug topic và user id, nên sẽ tìm session theo 2 thông tin này để đảm bảo tính linh hoạt cho client
                topicAggregate = await _topicRepository.GetBySlugAsync(request.SlugTopic, cancellationToken);
                if (topicAggregate is null)                
                {
                    return ThrowResultTopicNotFound();
                }
                sessionAggregate = await _userTopicExerciseSessionRepository.GetByTopicIdAndUserIdAsync(topicAggregate.TopicId, request.UserId, cancellationToken);
                if (sessionAggregate is null)                
                {
                    return ThrowResultSessionNotFound();
                }
            }
            else
            {
                sessionAggregate = await _userTopicExerciseSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
                if (sessionAggregate is null)
                {
                    return ThrowResultSessionNotFound();
                }
                if (sessionAggregate.UserId != request.UserId)
                {
                    return ThrowResultUnauthorized();
                }
            }
            // load các attempt và tính toán điểm số, trạng thái session bằng sessionId
            var exerciseAttemptsAggregate = await _exerciseAttemptRepository.GetAllBySessionIdAsync(sessionAggregate.SessionId, cancellationToken);
            var exerciseAttemptDict = exerciseAttemptsAggregate.ToDictionary(a => a.ExerciseId);

            // use session items for the canonical list of exercises in the session
            var exerciseIds = sessionAggregate.SessionItems.Select(i => i.ExerciseId).Distinct().ToList();
            var exercisesWithAnswers = await exerciseRepository.GetExerciseWithAnswersAsync(exerciseIds, cancellationToken);
            var exerciseWithAnswerDict = exercisesWithAnswers.ToDictionary(e => e.ExerciseId, e => e);

            var gradedCount = 0;
            var totalExercises = sessionAggregate.SessionItems.Count;
            var totalCorrect = 0;
            var totalWrong = 0;
            var listeningCorrect = 0;
            var readingCorrect = 0;
            var attemptScoreItems = new List<ExerciseAttemptBatchScoredItemDTO>();
            
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                sessionAggregate.SetTimeSpent((int)(DateTime.UtcNow - sessionAggregate.StartedAt).TotalSeconds);

                sessionAggregate.SessionItems.ToList().ForEach(sessionItem =>
                {
                    if (!exerciseAttemptDict.TryGetValue(sessionItem.ExerciseId, out var attempt))
                    {
                        return;
                    }

                    sessionItem.MarkCompleted(attempt.AttemptId);

                    if (!exerciseWithAnswerDict.TryGetValue(sessionItem.ExerciseId, out var exerciseRepo))
                    {
                        return;
                    }
                    var isCorrect = exerciseRepo.CorrectAnswer == attempt.Answer;
                    gradedCount++;
                    if (isCorrect)
                    {
                        totalCorrect++;
                        switch (attempt.SkillType)
                        {
                            case SkillType.Listening when exerciseRepo.SkillType == SkillType.Listening:
                                listeningCorrect++;
                                break;
                            case SkillType.Reading when exerciseRepo.SkillType == SkillType.Reading:
                                readingCorrect++;
                                break;
                        }
                        // Suppress per-attempt scored event to avoid event storm.
                        attempt.Finalize(100f, true, emitScoreEvent: false);
                    }
                    else
                    {
                        totalWrong++;
                        attempt.Finalize(0f, false, emitScoreEvent: false);
                    }
                    // add attempt score item for batch scoring event after session completion
                    attemptScoreItems.Add(new ExerciseAttemptBatchScoredItemDTO(
                        AttemptId: attempt.AttemptId,
                        ExerciseId: attempt.ExerciseId,
                        SkillType: attempt.SkillType,
                        Score: attempt.Score,
                        IsCorrect: attempt.IsCorrect,
                        CorrectAnswer: exerciseRepo.CorrectAnswer,
                        Options: exerciseRepo.Options,
                        ExerciseType: exerciseRepo.ExerciseType,
                        Difficulty: exerciseRepo.Difficulty,
                        Explanation: exerciseRepo.Explanation!,
                        AudioUrl: exerciseRepo.AudioUrl,
                        ImageUrl: exerciseRepo.ImageUrl,
                        Question: exerciseRepo.Question,
                        Description: exerciseRepo.Description!,
                        UpdatedAt: attempt.UpdatedAt
                    ));
                });

                sessionAggregate.SetScoreListening(listeningCorrect);
                sessionAggregate.SetScoreReading(readingCorrect);
                sessionAggregate.SetScoreWriting(0);
                sessionAggregate.SetTotalCorrect(totalCorrect);
                sessionAggregate.SetTotalWrong(totalWrong);
                sessionAggregate.SetTotalScore(gradedCount == 0 ? 0f : ((float)totalCorrect / totalExercises) * 100f);

                sessionAggregate.FinishSession();
                sessionAggregate.AddBatchScoreEvent(attemptScoreItems, gradedCount, totalExercises - gradedCount);

                // push domain events for session completion and attempt updates
                foreach (var domainEvent in sessionAggregate.DomainEvents)
                {
                    _logger.LogInformation("Publishing {EventType} for session completion. UserId: {UserId}, SessionId: {SessionId}", domainEvent.GetType().Name, request.UserId, sessionAggregate.SessionId);
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                sessionAggregate.PopDomainEvents();
            }, cancellationToken);

            return Result<CompleteLearningSessionResponseDTO>.SuccessResult(new CompleteLearningSessionResponseDTO(
                sessionAggregate.SessionId,
                request.SlugTopic,
                request.UserId,
                sessionAggregate.TotalScore ?? 0f,
                sessionAggregate.TotalCorrect,
                sessionAggregate.TotalWrong,
                sessionAggregate.ScoreListening,
                sessionAggregate.ScoreReading,
                sessionAggregate.TimeSpentSeconds,
                sessionAggregate.CompletedAt!.Value
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while submitting learning session {SessionId} for topic {SlugTopic} by user {UserId}", request.SessionId, request.SlugTopic, request.UserId);
            return Result<CompleteLearningSessionResponseDTO>.FailureResult("An error occurred while submitting the learning session. Please try again later.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }

    private static Result<CompleteLearningSessionResponseDTO> ThrowResultSessionNotFound()
    {
        return Result<CompleteLearningSessionResponseDTO>.FailureResult("Learning session item not found for the user and topic", (int)ErrorCode.NOTFOUND);
    }
    private static Result<CompleteLearningSessionResponseDTO> ThrowResultTopicNotFound()
    {
        return Result<CompleteLearningSessionResponseDTO>.FailureResult("Topic not found", (int)ErrorCode.NOTFOUND);
    }
    private static Result<CompleteLearningSessionResponseDTO> ThrowResultUnauthorized()
    {
        return Result<CompleteLearningSessionResponseDTO>.FailureResult("Unauthorized", (int)ErrorCode.UN_AUTHORIZED);
    }
}