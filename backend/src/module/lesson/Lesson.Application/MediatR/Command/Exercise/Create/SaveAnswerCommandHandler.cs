namespace Lesson.Application.MediatR.Command.Exercise.Create;

public sealed record SaveAnswerCommand(
    Guid ExerciseId,
    Guid SessionId,
    Guid UserId,
    string Answer,
    int CurrentSequenceNo
) : IRequest<Result<SaveAnswerResponseDTO>>;

#region Validator
public class ValidatorSaveAnswer : AbstractValidator<SaveAnswerCommand>
{
    public ValidatorSaveAnswer()
    {
        RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("ExerciseId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.Answer).NotEmpty().WithMessage("Answer is required.");
        RuleFor(x => x.CurrentSequenceNo).GreaterThanOrEqualTo(0).WithMessage("CurrentSequenceNo must be a non-negative integer.");
    }
}
#endregion

public sealed class SaveAnswerCommandHandler(IExerciseRepository exerciseRepository, ILessonUnitOfWork unitOfWork, ILogger<SaveAnswerCommandHandler> logger, IPublisher publisher, IUserTopicExerciseSessionRepository userTopicExerciseSessionRepository, IExerciseAttemptRepository exerciseAttemptRepository) : IRequestHandler<SaveAnswerCommand, Result<SaveAnswerResponseDTO>>
{
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<SaveAnswerCommandHandler> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly IExerciseAttemptRepository _exerciseAttemptRepository = exerciseAttemptRepository ?? throw new ArgumentNullException(nameof(exerciseAttemptRepository));
    private readonly IUserTopicExerciseSessionRepository _userTopicExerciseSessionRepository = userTopicExerciseSessionRepository ?? throw new ArgumentNullException(nameof(userTopicExerciseSessionRepository));

    // sẽ chia làm 2 loại, vừa tạo và vừa cập nhật lại attempt nếu đã tồn tại (cho phép sửa đáp án trước khi chấm điểm)
    public async Task<Result<SaveAnswerResponseDTO>> Handle(SaveAnswerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var exerciseAggregate = null as ExerciseAggregate;
            var sessionExercise = null as UserTopicExerciseSessionAggregate;
            // var 
            exerciseAggregate = await _exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken);
            if (exerciseAggregate is null) 
            {
                return Result<SaveAnswerResponseDTO>.FailureResult("Exercise not found", (int)ErrorCode.NOTFOUND);
            }
            // phase 1: validate input (đã có FluentValidation đảm nhiệm, nhưng nếu cần validate thêm phức tạp hơn thì có thể làm ở đây)
            sessionExercise = await _userTopicExerciseSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionExercise is null)
            {
                return Result<SaveAnswerResponseDTO>.FailureResult("Learning session item not found", (int)ErrorCode.NOTFOUND);
            }

            if (sessionExercise.UserId != request.UserId)
            {
                return Result<SaveAnswerResponseDTO>.FailureResult("Unauthorized", (int)ErrorCode.UN_AUTHORIZED);
            }

            var exerciseAttempt = await _exerciseAttemptRepository.GetByExerciseIdAndSessionIdAsync(request.ExerciseId, request.SessionId, cancellationToken);
            var isNewAttempt = exerciseAttempt is null;
            if (isNewAttempt)
            {
                exerciseAttempt = ExerciseAttemptAggregate.Create(
                    sessionId: request.SessionId,
                    exerciseId: request.ExerciseId,
                    answer: request.Answer,
                    skillType: exerciseAggregate.SkillType
                );
            }

            
            else
            {
                if (exerciseAttempt!.IsFinalized)
                {
                    return Result<SaveAnswerResponseDTO>.FailureResult("Exercise attempt already finalized, cannot submit answer", (int)ErrorCode.BAD_REQUEST);
                }

                if (exerciseAttempt.SessionId != request.SessionId)
                {
                    return Result<SaveAnswerResponseDTO>.FailureResult("Exercise does not match with session", (int)ErrorCode.BAD_REQUEST);
                }

                exerciseAttempt.UpdateAnswer(request.Answer, request.ExerciseId, request.SessionId);
            }
            // update currentSequenceNo in sessionExercise if needed
            if (request.CurrentSequenceNo > sessionExercise.CurrentSequenceNo)
            {
                sessionExercise.UpdateCurrentSequenceNo(request.CurrentSequenceNo);
                // invalid cache của sessionExercise để client có thể lấy được currentSequenceNo mới nhất
                
            }

            await _unitOfWork.SaveChangeAsync(
                async () =>
                {
                    if (exerciseAttempt.AttemptId == Guid.Empty)
                    {
                        throw new InvalidOperationException("Exercise attempt id is invalid.");
                    }
                    await _userTopicExerciseSessionRepository.UpdateAsync(sessionExercise, cancellationToken);

                    if (isNewAttempt)
                    {
                        await _exerciseAttemptRepository.AddAsync(exerciseAttempt, cancellationToken);
                    }
                    else
                    {
                        await _exerciseAttemptRepository.UpdateAsync(exerciseAttempt, cancellationToken);
                    }

                    foreach (var domainEvent in exerciseAttempt.DomainEvents)
                    {
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }

                    exerciseAttempt.PopDomainEvents();
                }, cancellationToken);

            return Result<SaveAnswerResponseDTO>.SuccessResult(new SaveAnswerResponseDTO(
                ExerciseId: exerciseAttempt.ExerciseId,
                SessionId: exerciseAttempt.SessionId,
                AnsweredAt: exerciseAttempt.UpdatedAt,
                Status: "Saved",
                CurrentSequenceNo: sessionExercise.CurrentSequenceNo
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while submitting answer for exercise {ExerciseId} in session {SessionId} by user {UserId}", request.ExerciseId, request.SessionId, request.UserId);
            return Result<SaveAnswerResponseDTO>.FailureResult("An error occurred while submitting the answer. Please try again later.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}