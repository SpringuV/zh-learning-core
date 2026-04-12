using Lesson.Domain.Entities.Events;

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
            await _unitOfWork.SaveChangeAsync(async () =>
            {
                // 1 query thay vì N queries
                var courses = await _courseRepository.GetByIdsAsync(
                    request.OrderedCourseIds, 
                    cancellationToken);
                
                if (courses.Count() != request.OrderedCourseIds.Count)
                    throw new KeyNotFoundException("Một số khóa học không tồn tại");

                // Update tất cả OrderIndex
                for (int i = 0; i < courses.Count(); i++)
                {
                    courses.ElementAt(i).UpdateOrderIndex(i + 1);
                }

                // 1 update batch thay vì N updates
                await _courseRepository.UpdateRangeAsync(courses, cancellationToken);

                //  Publish 1 aggregated event thay vì N events
                var reorderEvent = new CourseReOrderedEvent(
                    OrderedCourseIds: request.OrderedCourseIds,
                    UpdatedAt: DateTime.UtcNow
                );
                await _publisher.Publish(reorderEvent, cancellationToken);

            }, cancellationToken);

            return Result.SuccessResult(message: "Reorder successfully.");
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