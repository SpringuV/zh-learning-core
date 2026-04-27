using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record ExerciseSessionPracticeItemWithoutAnswerQueries(Guid ExerciseId) : IRequest<Result<ExerciseSessionPracticeItemWithoutAnswerResponse>>;

public class ExerciseSessionPracticeItemWithoutAnswerQueriesHandler(ElasticsearchClient client, ILogger<ExerciseSessionPracticeItemWithoutAnswerQueriesHandler> logger) : IRequestHandler<ExerciseSessionPracticeItemWithoutAnswerQueries, Result<ExerciseSessionPracticeItemWithoutAnswerResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<ExerciseSessionPracticeItemWithoutAnswerQueriesHandler> _logger = logger;

    public async Task<Result<ExerciseSessionPracticeItemWithoutAnswerResponse>> Handle(ExerciseSessionPracticeItemWithoutAnswerQueries request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing exercise session practice item without answer search with request: {@SearchRequest}", request);
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.ExerciseIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning not found result.", ConstantIndexElastic.ExerciseIndex);
                return Result<ExerciseSessionPracticeItemWithoutAnswerResponse>.FailureResult("Exercise not found", (int)ErrorCode.NOTFOUND);
            }
            var response = await _client.SearchAsync<ExerciseSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.ExerciseIndex);
                s.Size(1);
                s.Query(q => q
                    .Term(t => t
                        .Field(f => f.ExerciseId.Suffix("keyword"))
                        .Value(request.ExerciseId.ToString())
                    )
                );
            }, cancellationToken);

            // Hits.FirstOrDefault() sẽ trả về null nếu không có kết quả nào, nên cần kiểm tra trước khi truy cập Source
            // Source sẽ chứa dữ liệu của document nếu tìm thấy, nếu không sẽ là null
            var exercise = response.Hits.FirstOrDefault()?.Source;
            if (exercise is null)
            {
                _logger.LogInformation("Exercise with ID {ExerciseId} not found in index. Returning not found result.", request.ExerciseId);
                return Result<ExerciseSessionPracticeItemWithoutAnswerResponse>.FailureResult("Exercise not found", (int)ErrorCode.NOTFOUND);
            }

            var result = new ExerciseSessionPracticeItemWithoutAnswerResponse(
                ExerciseId: exercise.ExerciseId,
                Question: exercise.Question,
                ExerciseType: exercise.ExerciseType.ToString(),
                SkillType: exercise.SkillType.ToString(),
                Difficulty: exercise.Difficulty.ToString(),
                Description: exercise.Description,
                OrderIndex: exercise.OrderIndex,
                Options: [.. exercise.Options.Select(o => new ExerciseOption(o.Id, o.Text))],
                AudioUrl: exercise.AudioUrl,
                ImageUrl: exercise.ImageUrl,
                Slug: exercise.Slug
            );

            return Result<ExerciseSessionPracticeItemWithoutAnswerResponse>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for exercise session practice item without answer with request: {@SearchRequest}", request);
            return Result<ExerciseSessionPracticeItemWithoutAnswerResponse>.FailureResult("An error occurred while retrieving the exercise", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}