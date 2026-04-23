using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface ITopicProgressQueriesService
{
    Task HandleStartedLearningAsync(TopicProgressCreatedQueriesRequest request, CancellationToken cancellationToken = default);
    Task HandleExerciseSessionStartedAsync(ExerciseSessionStartedQueriesRequest request, CancellationToken cancellationToken = default);
    Task HandleExerciseSessionSnapshotInitializedAsync(ExerciseSessionSnapshotInitializedQueriesRequest request, CancellationToken cancellationToken = default);
}