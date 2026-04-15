namespace Lesson.Application.MediatR.Command.Exercise.Update;

public record UpdateExerciseCommand(
    Guid ExerciseId,
    string? Description,
    string? Question,
    string? CorrectAnswer,
    ExerciseType? ExerciseType,
    SkillType? SkillType,
    ExerciseDifficulty? Difficulty,
    ExerciseContext? ExerciseContext,
    string? AudioUrl,
    string? ImageUrl,
    string? Explanation,
    IReadOnlyList<ExerciseOption>? Options
) : IRequest<Result>;

public class UpdateExerciseCommandHandler(
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        IExerciseRepository exerciseRepository,
        ILogger<UpdateExerciseCommandHandler> logger) 
    : IRequestHandler<UpdateExerciseCommand, Result>
{
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ILogger<UpdateExerciseCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                var exerciseAggregate = await _exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken);
                if (exerciseAggregate is null)
                {
                    throw new KeyNotFoundException($"Exercise with id '{request.ExerciseId}' was not found.");
                }

                // Update fields if provided
                if (request.Description is not null)
                    exerciseAggregate.UpdateDescription(request.Description);
                if (request.Question is not null)
                    exerciseAggregate.UpdateQuestion(request.Question);
                if (request.CorrectAnswer is not null)
                    exerciseAggregate.UpdateCorrectAnswer(request.CorrectAnswer);
                if (request.ExerciseType is not null)
                    exerciseAggregate.UpdateExerciseType(request.ExerciseType.Value);
                if (request.SkillType is not null)
                    exerciseAggregate.UpdateSkillType(request.SkillType.Value);
                if (request.Difficulty is not null)
                    exerciseAggregate.UpdateDifficulty(request.Difficulty.Value);
                if (request.ExerciseContext is not null)
                    exerciseAggregate.UpdateExerciseContext(request.ExerciseContext.Value);
                if (request.AudioUrl is not null)
                    exerciseAggregate.UpdateAudioUrl(request.AudioUrl);
                if (request.ImageUrl is not null)
                    exerciseAggregate.UpdateImageUrl(request.ImageUrl);

                if (request.Explanation is not null)
                    exerciseAggregate.UpdateExplanation(request.Explanation);
                if (request.Options is not null)
                    exerciseAggregate.UpdateOptions([.. request.Options]);

                await _exerciseRepository.UpdateAsync(exerciseAggregate, cancellationToken);

                _logger.LogInformation("[UpdateExerciseCommandHandler] Publishing {EventCount} domain events in-transaction", exerciseAggregate.DomainEvents.Count);
                foreach (var domainEvent in exerciseAggregate.DomainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                exerciseAggregate.PopDomainEvents();
            }, cancellationToken);

            return Result.SuccessResult("Exercise updated successfully.");
        }
        catch (KeyNotFoundException)
        {
            return Result.FailureResult(
                $"Exercise with id '{request.ExerciseId}' was not found.",
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (ArgumentException ex)
        {
            return Result.FailureResult(ex.Message, (int)ErrorCode.VALIDATION);
        }
        catch (InvalidOperationException ex)
        {
            return Result.FailureResult(ex.Message, (int)ErrorCode.INVALID_STATE);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating exercise {ExerciseId}", request.ExerciseId);
            return Result.FailureResult("Unexpected error while updating exercise.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}