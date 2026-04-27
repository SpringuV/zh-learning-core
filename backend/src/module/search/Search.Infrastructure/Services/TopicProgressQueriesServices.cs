namespace Search.Infrastructure.Services;

public class TopicProgressQueriesServices(IMediator mediator) : ITopicProgressQueriesService
{
    private readonly IMediator _mediator = mediator;

    public Task HandleExerciseSessionSnapshotInitializedAsync(ExerciseSessionSnapshotInitializedQueriesRequest request, CancellationToken cancellationToken = default)
        => _mediator.Send(
            new TopicExerciseSessionSnapshotInitializedCommand(
                SessionId: request.SessionId,
                UserId: request.UserId,
                TopicId: request.TopicId,
                TotalExercises: request.TotalExercises,
                CurrentSequenceNo: request.CurrentSequenceNo,
                HskLevel: request.HskLevel,
                ExerciseItems: request.SessionItems,
                InitializedAt: request.InitializedAt,
                UpdatedAt: request.UpdatedAt),
            cancellationToken);

    public Task HandleExerciseSessionStartedAsync(ExerciseSessionStartedQueriesRequest request, CancellationToken cancellationToken = default)
        => _mediator.Send(
            new ExerciseSessionStartedCommand(
                SessionId: request.SessionId,
                UserId: request.UserId,
                HskLevel: request.HskLevel,
                TopicId: request.TopicId,
                Status: request.Status,
                UpdatedAt: request.StartedAt),
            cancellationToken);

    public Task HandleStartedLearningAsync(TopicProgressCreatedQueriesRequest request, CancellationToken cancellationToken = default)
        => _mediator.Send(
            new TopicProgressIndexCommand(
                TopicProgressId: request.TopicProgressId,
                UserId: request.UserId,
                TopicId: request.TopicId,
                TotalAttempts: request.TotalAttempts,
                TotalAnswered: request.TotalAnswered,
                TotalCorrect: request.TotalCorrect,
                TotalWrong: request.TotalWrong,
                TotalScore: request.TotalScore,
                AccuracyRate: request.AccuracyRate,
                CreatedAt: request.CreatedAt),
            cancellationToken);
}