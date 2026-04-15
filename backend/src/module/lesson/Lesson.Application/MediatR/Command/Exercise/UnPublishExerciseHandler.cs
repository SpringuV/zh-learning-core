namespace Lesson.Application.MediatR.Command.Exercise;

public record UnPublishExerciseCommand(Guid ExerciseId) : IRequest<Result>;

public class UnPublishExerciseHandler(IExerciseRepository exerciseRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<UnPublishExerciseHandler> logger) : IRequestHandler<UnPublishExerciseCommand, Result>
{
    private readonly ILogger<UnPublishExerciseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(UnPublishExerciseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            ExerciseAggregate? exerciseAggregate = null;
            
            if (request.ExerciseId == Guid.Empty)
                return Result.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                exerciseAggregate = await _exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken);
                if (exerciseAggregate is not null)
                {
                    exerciseAggregate.UnPublish();
                    await _exerciseRepository.UpdateAsync(exerciseAggregate, cancellationToken);

                    foreach (var domainEvent in exerciseAggregate.DomainEvents)
                    {
                        _logger.LogInformation("Publishing domain event {EventType} for exercise {ExerciseId}", domainEvent.GetType().Name, exerciseAggregate.ExerciseId);
                        await _publisher.Publish(domainEvent, cancellationToken);
                    }
                    exerciseAggregate.PopDomainEvents();
                }
                else
                    throw new KeyNotFoundException($"Không tìm thấy bài tập với ID: {request.ExerciseId}");
            }, cancellationToken);

            return Result.SuccessResult("Bài tập đã được hủy xuất bản thành công.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Exercise not found with ID: {ExerciseId}", request.ExerciseId);
            return Result.FailureResult(
                "Không tìm thấy bài tập với ID: " + request.ExerciseId,
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Exercise with ID {ExerciseId} is already published.", request.ExerciseId);
            return Result.FailureResult(
                "Bài tập với ID " + request.ExerciseId + " chưa được xuất bản.",
                (int)ErrorCode.NOT_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during exercise publish operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                "Đã xảy ra lỗi không mong muốn trong quá trình hủy xuất bản bài tập.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}