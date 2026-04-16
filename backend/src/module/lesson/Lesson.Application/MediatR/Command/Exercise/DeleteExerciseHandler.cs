namespace Lesson.Application.MediatR.Command.Exercise;

public record DeleteExerciseCommand(Guid ExerciseId) : IRequest<Result>;

public class DeleteExerciseCommandHandler(
        ILessonUnitOfWork unitOfWork, 
        IPublisher publisher, 
        IExerciseRepository exerciseRepository,
        ILogger<DeleteExerciseCommandHandler> logger) 
    : IRequestHandler<DeleteExerciseCommand, Result>
{
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ILogger<DeleteExerciseCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                var exerciseAggregate = await _exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken) ?? throw new KeyNotFoundException($"Exercise with id '{request.ExerciseId}' was not found.");
                exerciseAggregate.Delete();
                await _exerciseRepository.UpdateAsync(exerciseAggregate, cancellationToken);

                // Publish domain events
                var domainEvents = exerciseAggregate.DomainEvents;
                foreach (var domainEvent in domainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                exerciseAggregate.PopDomainEvents();
            }, cancellationToken);

            return Result.SuccessResult();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Exercise not found with ID: {ExerciseId}", request.ExerciseId);
            return Result.FailureResult(
                "Không tìm thấy bài tập.",
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete exercise with ID {ExerciseId} due to invalid state.", request.ExerciseId);
            return Result.FailureResult(
                ex.Message, // Sử dụng thông báo lỗi chi tiết từ exception để trả về cho client, vì trong trường hợp này có thể có nhiều lý do khác nhau khiến việc xóa bài tập không hợp lệ như đã được xuất bản, đang được sử dụng trong một chủ đề nào đó, ...
                (int)ErrorCode.INVALID_OPERATION
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during exercise delete operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                "Đã xảy ra lỗi không mong muốn trong quá trình xóa bài tập.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}