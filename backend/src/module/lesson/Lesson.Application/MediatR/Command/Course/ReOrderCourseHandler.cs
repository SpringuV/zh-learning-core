namespace Lesson.Application.MediatR.Command.Course;

public record ReOrderCourseCommand(List<Guid> OrderedCourseIds) : IRequest<Result>;

public class ReOrderCourseHandler(ICourseRepository courseRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<ReOrderCourseHandler> logger): IRequestHandler<ReOrderCourseCommand, Result>
{
    private readonly ILogger<ReOrderCourseHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    public async Task<Result> Handle(ReOrderCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.OrderedCourseIds.Count == 0)
                throw new ArgumentException("Danh sách khóa học sắp xếp không được để trống.");

            if (request.OrderedCourseIds.Count != request.OrderedCourseIds.Distinct().Count())
                throw new ArgumentException("Danh sách khóa học sắp xếp không được chứa ID trùng lặp.");

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                // Reorder bằng 1 SQL command (2-phase trong CTE) để tránh va chạm unique index.
                await _courseRepository.ReorderByIdsAsync(
                    request.OrderedCourseIds,
                    cancellationToken);

                //  Publish 1 aggregated event thay vì N events
                var reorderEvent = new CourseReOrderedEvent(
                    OrderedCourseIds: request.OrderedCourseIds,
                    UpdatedAt: DateTime.UtcNow
                );
                await _publisher.Publish(reorderEvent, cancellationToken);

            }, cancellationToken);

            return Result.SuccessResult(message: "Reorder successfully.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid reorder request. Details: {Message}", ex.Message);
            return Result.FailureResult("Dữ liệu sắp xếp không hợp lệ. " + ex.Message, (int)ErrorCode.INVALID_ARGUMENT);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Course not found during reorder operation. Details: {Message}", ex.Message);
            return Result.FailureResult("Không tìm thấy khóa học trong quá trình sắp xếp. " + ex.Message, (int)ErrorCode.NOTFOUND);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during course reorder operation. Details: {Message}", ex.Message);
            return Result.FailureResult("Đã xảy ra lỗi không mong muốn trong quá trình sắp xếp khóa học.", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}