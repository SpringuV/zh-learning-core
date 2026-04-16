using Search.Infrastructure.Queries.Lesson.Delete;
using Search.Infrastructure.Queries.Lesson.Update.Exercise;

namespace Search.Infrastructure.Services;

public class ExerciseSearchQueriesService(IMediator mediator) : IExerciseSearchQueriesService
{
    private readonly IMediator _mediator = mediator;

    private Task SendPatchAsync(Guid exerciseId, object patch, CancellationToken cancellationToken)
        => _mediator.Send(new ExercisePatchSearchCommand(exerciseId, patch), cancellationToken);

    public Task UpdateExerciseTypeAsync(ExerciseTypeUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                ExerciseType = request.NewExerciseType,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateExerciseSkillTypeAsync(ExerciseSkillTypeUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                SkillType = request.NewSkillType,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateExerciseContextAsync(ExerciseContextUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                Context = request.NewContext,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateDescriptionAsync(ExerciseDescriptionUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                Description = request.NewDescription,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateQuestionAsync(ExerciseQuestionUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                Question = request.NewQuestion,
                Slug = request.NewSlug,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateCorrectAnswerAsync(ExerciseCorrectAnswerUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                CorrectAnswer = request.NewCorrectAnswer,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateDifficultyAsync(ExerciseDifficultyUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                Difficulty = request.NewDifficulty,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateAudioUrlAsync(ExerciseAudioUrlUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                AudioUrl = request.NewAudioUrl,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateImageUrlAsync(ExerciseImageUrlUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                ImageUrl = request.NewImageUrl,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateExplanationAsync(ExerciseExplanationUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                Explanation = request.NewExplanation,
                request.UpdatedAt
            },
            cancellationToken);

    public Task UpdateOptionsAsync(ExerciseOptionsUpdatedRequestDTO request, CancellationToken cancellationToken = default)
        => SendPatchAsync(
            request.ExerciseId,
            new
            {
                Options = request.NewOptions,
                request.UpdatedAt
            },
            cancellationToken);

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

    public async Task DeleteAsync(ExerciseDeletedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new ExerciseDeletedSearchCommand(
            ExerciseId: request.ExerciseId
        );
        await _mediator.Send(command, cancellationToken);
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

    public async Task PublishAsync(ExercisePublishedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new ExercisePublishedSearchCommand(
            ExerciseId: request.ExerciseId,
            PublishedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task UnPublishAsync(ExerciseUnPublishedRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new ExerciseUnPublishedSearchCommand(
            ExerciseId: request.ExerciseId,
            UnpublishedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task ReOrderAsync(ExerciseReorderSearchRequestDTO request, CancellationToken cancellationToken = default)
    {
        var command = new ExerciseReOrderSearchCommand(
            TopicId: request.TopicId,
            OrderedExerciseIds: request.OrderedExerciseIds,
            UpdatedAt: request.UpdatedAt
        );
        await _mediator.Send(command, cancellationToken);
    }

    public async Task<ExerciseSearchWithTopicMetadataResponse> SearchExerciseAdminWithTopicMetadataAsync(ExerciseSearchQueryRequest request, CancellationToken cancellationToken = default)
    {
        var queries = new TopicExercisesOverviewAdminQueries(
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