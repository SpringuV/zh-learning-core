namespace Lesson.Application.MediatR.Command.Topic;

public sealed record CreateTopicCommand(
    Guid CourseId,
    string Title,
    string Description,
    string TopicType,
    int EstimatedTimeMinutes,
    int? ExamYear = null,
    string? ExamCode = null
) : IRequest<Result<CreateTopicResponseDTO>>;

public class CreateTopicHandler(ITopicRepository topicRepository, ILogger<CreateTopicHandler> logger, IPublisher publisher, ILessonUnitOfWork unitOfWork) : IRequestHandler<CreateTopicCommand, Result<CreateTopicResponseDTO>>
{
    private readonly ITopicRepository _topicRepository = topicRepository ?? throw new ArgumentNullException(nameof(topicRepository));
    private readonly ILogger<CreateTopicHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private ILessonUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    public async Task<Result<CreateTopicResponseDTO>> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        try {
            if (!Enum.TryParse<TopicType>(request.TopicType, true, out var parsedTopicType) ||
            !Enum.IsDefined(parsedTopicType))
        {
            return Result<CreateTopicResponseDTO>.FailureResult(
                $"TopicType '{request.TopicType}' không hợp lệ. Giá trị hỗ trợ: {string.Join(", ", Enum.GetNames<TopicType>())}.",
                (int)ErrorCode.INVALID_ENUM
            );
        }
        TopicAggregate topicAggregate = null!;
        var maxOrderIndex = await _topicRepository.GetMaxOrderIndexAsync(cancellationToken);
        var currentMaxOrderIndex = maxOrderIndex ?? 0;
        await _unitOfWork.SaveChangeAsync(async() => {
            topicAggregate = TopicAggregate.CreateTopic(
                title: request.Title,
                description: request.Description,
                courseId: request.CourseId,
                topicType: parsedTopicType,
                estimatedTimeMinutes: request.EstimatedTimeMinutes,
                orderIndex: currentMaxOrderIndex + 1,

                examYear: request.ExamYear ?? null,
                examCode: request.ExamCode ?? string.Empty
            );

            await _topicRepository.AddAsync(topicAggregate, cancellationToken);
            _logger.LogInformation("[CreateTopicHandler] Aggregate added to repository with TopicId: {TopicId}", topicAggregate.TopicId);
            foreach (var domainEvent in topicAggregate.DomainEvents)
            {
                _logger.LogInformation("[CreateTopicHandler] Publishing {EventType} for {AggregateId}", domainEvent.GetType().Name, topicAggregate.TopicId);
                await _publisher.Publish(domainEvent, cancellationToken);
            }
            topicAggregate.PopDomainEvents();
            
        }, cancellationToken);
        return Result<CreateTopicResponseDTO>.SuccessResult(
                new CreateTopicResponseDTO(TopicId: topicAggregate.TopicId), 
                "Tạo chủ đề thành công.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument when creating topic: {Message}", ex.Message);
            return Result<CreateTopicResponseDTO>.FailureResult($"Dữ liệu không hợp lệ: {ex.Message}", (int)ErrorCode.VALIDATION);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Repository error when creating topic: {Message}", ex.Message);
            return Result<CreateTopicResponseDTO>.FailureResult($"Lỗi lưu trữ dữ liệu: {ex.Message}", (int)ErrorCode.DUPLICATE);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operation cancelled when creating topic");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when creating topic: {Message}", ex.Message);
            return Result<CreateTopicResponseDTO>.FailureResult($"Lỗi không xác định: {ex.Message}", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}