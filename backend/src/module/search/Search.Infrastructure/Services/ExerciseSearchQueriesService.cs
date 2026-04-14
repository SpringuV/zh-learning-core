namespace Search.Infrastructure.Services;

public class ExerciseSearchQueriesService(IMediator mediator) : IExerciseSearchQueriesService
{
    private readonly IMediator _mediator = mediator;
    public async Task<ExerciseIndexResponse> IndexAsync(ExerciseSearchIndexQueriesRequest request, CancellationToken cancellationToken = default)
    {
        var command = new ExerciseIndexCommand(
            ExerciseId: request.ExerciseId,
            TopicId: request.TopicId,
            Question: request.Question,
            Description: request.Description,
            OrderIndex: request.OrderIndex,
            Slug: request.Slug,
            CorrectAnswer: request.CorrectAnswer,
            ExerciseType: request.ExerciseType,
            SkillType: request.SkillType,
            Difficulty: request.Difficulty,
            Context: request.Context,
            AudioUrl: request.AudioUrl,
            ImageUrl: request.ImageUrl,
            Explanation: request.Explanation,
            IsPublished: request.IsPublished,
            CreatedAt: request.CreatedAt,
            UpdatedAt: request.UpdatedAt,
            Options: request.Options);
        return await _mediator.Send(command, cancellationToken);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SearchQueryResult<ExerciseSearchItemAdminResponse>> SearchExerciseItemAdminAsync(ExerciseSearchQueryRequest request, CancellationToken cancellationToken = default)
    {
        var queries = new ExerciseSearchAdminQueries(
            TopicId: request.TopicId,
            Question: request.Question,
            IsPublished: request.IsPublished,
            SkillType: request.SkillType,
            ExerciseType: request.ExerciseType,
            Difficulty: request.Difficulty,
            Context: request.Context,
            StartCreatedAt: request.StartCreatedAt,
            EndCreatedAt: request.EndCreatedAt,
            Take: request.Take,
            SearchAfterValues: request.SearchAfterValues,
            SortBy: request.SortBy,
            OrderByDescending: request.OrderByDescending
        );
        return await _mediator.Send(queries, cancellationToken);
    }
}