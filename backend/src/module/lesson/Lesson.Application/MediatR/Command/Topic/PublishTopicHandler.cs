namespace Lesson.Application.MediatR.Command.Topic;

public record PublishTopicCommand(Guid TopicId) : IRequest<Result>;

public class PublishTopicHandler(ICourseRepository courseRepository, IExerciseRepository exerciseRepository, ITopicRepository topicRepository, ILessonUnitOfWork unitOfWork, IPublisher publisher, ILogger<PublishTopicHandler> logger) : IRequestHandler<PublishTopicCommand, Result>
{
    private readonly ICourseRepository _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly ILogger<PublishTopicHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    
    public async Task<Result> Handle(PublishTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            TopicAggregate? topicAggregate = null;
            
            if (request.TopicId == Guid.Empty)
                return Result.FailureResult("Id không được để trống.", (int)ErrorCode.INVALID_ID);

            await _unitOfWork.SaveChangeAsync(async () =>
            {
                topicAggregate = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken) ?? throw new KeyNotFoundException($"Topic with id '{request.TopicId}' was not found.");
                // check exercise với id topic này, nếu có bài tập nào đã được xuất bản thì mới cho phép xuất bản chủ đề, nếu không sẽ trả về lỗi, tránh trường hợp xuất bản chủ đề nhưng chưa có bài tập nào được xuất bản dẫn đến trải nghiệm người dùng kém khi họ vào học mà không thấy bài tập nào.
                if (topicAggregate.TotalExercises == 0) 
                    throw new InvalidOperationException("Không thể xuất bản chủ đề khi chưa có bài tập nào được xuất bản. Vui lòng xuất bản ít nhất một bài tập trước khi xuất bản chủ đề.");
                // check exercise đã xuất bản nào có id topic này chưa, nếu chưa có thì không cho phép xuất bản chủ đề, tránh trường hợp xuất bản chủ đề nhưng chưa có bài tập nào được xuất bản dẫn đến trải nghiệm người dùng kém khi họ vào học mà không thấy bài tập nào.
                var hasPublishedExercise = await _exerciseRepository.HasPublishedExerciseAsync(request.TopicId, cancellationToken);
                if (!hasPublishedExercise)                {
                    throw new InvalidOperationException("Không thể xuất bản chủ đề khi chưa có bài tập nào được xuất bản. Vui lòng xuất bản ít nhất một bài tập trước khi xuất bản chủ đề.");
                }
                // ++ increse totalTopicsPublished của course
                var course = await _courseRepository.GetByIdAsync(topicAggregate.CourseId, cancellationToken);
                if (course is null)                {
                    throw new KeyNotFoundException($"Course with id '{topicAggregate.CourseId}' was not found for topic '{topicAggregate.TopicId}'.");
                }
                course.IncreaseTotalTopicsPublished();
                await _courseRepository.UpdateAsync(course, cancellationToken);
                topicAggregate.Publish();
                await _topicRepository.UpdateAsync(topicAggregate, cancellationToken);

                _logger.LogInformation("[PublishTopicHandler] Publishing {EventCount} domain events in-transaction", topicAggregate.DomainEvents.Count);
                foreach (var domainEvent in topicAggregate.DomainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                topicAggregate.PopDomainEvents();  
                foreach (var domainEvent in course.DomainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
                course.PopDomainEvents();
            }, cancellationToken);

            return Result.SuccessResult(
                "Chủ đề đã được xuất bản thành công."
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Topic not found with ID: {TopicId}", request.TopicId);
            return Result.FailureResult(
                "Không tìm thấy chủ đề.",
                (int)ErrorCode.NOTFOUND
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Topic with ID {TopicId} is already published.", request.TopicId);
            return Result.FailureResult(
                "Chủ đề đã được xuất bản.",
                (int)ErrorCode.ALREADY_PUBLISHED
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during topic publish operation. Details: {Message}", ex.Message);
            return Result.FailureResult(
                "Đã xảy ra lỗi không mong muốn trong quá trình xuất bản chủ đề.",
                (int)ErrorCode.INTERNAL_ERROR
            );
        }
    }
}