using Search.Contracts.DTOs;

namespace Search.Contracts.Interfaces;

public interface IExerciseSearchQueriesService
{
    Task<ExerciseIndexResponse> IndexAsync(ExerciseSearchIndexQueriesRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SearchQueryResult<ExerciseSearchItemAdminResponse>> SearchExerciseItemAdminAsync(ExerciseSearchQueryRequest request, CancellationToken cancellationToken = default);
}