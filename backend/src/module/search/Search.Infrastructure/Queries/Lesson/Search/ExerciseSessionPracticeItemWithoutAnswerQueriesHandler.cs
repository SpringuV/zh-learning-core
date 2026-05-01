using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record ExerciseSessionPracticeItemWithoutAnswerQueries(Guid ExerciseId, Guid UserId, Guid SessionId) : IRequest<Result<LearningExerciseSessionPracticeDTOResponse>>, ICacheableRequest<Result<LearningExerciseSessionPracticeDTOResponse>>, ICacheScopeRequest
{
    public string CacheKey => $"session-practice-item-wo-answer:{ExerciseId}:{UserId}:{SessionId}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);

    public string CacheScope => SearchCacheScopes.ExerciseSessionPracticeItemWithoutAnswer;
}

public class ExerciseSessionPracticeItemWithoutAnswerQueriesHandler(ElasticsearchClient client, ILogger<ExerciseSessionPracticeItemWithoutAnswerQueriesHandler> logger) : IRequestHandler<ExerciseSessionPracticeItemWithoutAnswerQueries, Result<LearningExerciseSessionPracticeDTOResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<ExerciseSessionPracticeItemWithoutAnswerQueriesHandler> _logger = logger;

    public async Task<Result<LearningExerciseSessionPracticeDTOResponse>> Handle(ExerciseSessionPracticeItemWithoutAnswerQueries request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing exercise session practice item without answer search with request: {@SearchRequest}", request);
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.ExerciseIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning not found result.", ConstantIndexElastic.ExerciseIndex);
                return Result<LearningExerciseSessionPracticeDTOResponse>.FailureResult("Exercise not found", (int)ErrorCode.NOTFOUND);
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
            // load thêm câu trả lời đã lưu của người dùng nếu có (để hiển thị lại khi người dùng tiếp tục làm bài tập đó, tránh trường hợp họ đã trả lời rồi nhưng chưa submit mà lại bị out ra ngoài do hết thời gian hoặc tắt app...)
            var responseAnswerd = await _client.SearchAsync<TopicExerciseAttemptSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.TopicExerciseAttemptIndex);
                s.Size(1);
                s.Query(q => q
                    .Bool(b => b
                        .Must(
                            mu => mu.Term(t => t.Field(f => f.ExerciseId.Suffix("keyword")).Value(request.ExerciseId.ToString())),
                            mu => mu.Term(t => t.Field(f => f.UserId.Suffix("keyword")).Value(request.UserId.ToString())),
                            mu => mu.Term(t => t.Field(f => f.SessionId.Suffix("keyword")).Value(request.SessionId.ToString()))
                        )
                    )
                );
            }, cancellationToken);

            // Hits.FirstOrDefault() sẽ trả về null nếu không có kết quả nào, nên cần kiểm tra trước khi truy cập Source
            // Source sẽ chứa dữ liệu của document nếu tìm thấy, nếu không sẽ là null
            var exercise = response.Hits.FirstOrDefault()?.Source;
            if (exercise is null)
            {
                _logger.LogInformation("Exercise with ID {ExerciseId} not found in index. Returning not found result.", request.ExerciseId);
                return Result<LearningExerciseSessionPracticeDTOResponse>.FailureResult("Exercise not found", (int)ErrorCode.NOTFOUND);
            }

            var result = new LearningExerciseSessionPracticeDTOResponse(
                ExerciseId: exercise.ExerciseId,
                Question: exercise.Question,
                SkillType: exercise.SkillType.ToString(),
                Difficulty: exercise.Difficulty.ToString(),
                Description: exercise.Description,
                Answerd: responseAnswerd.Hits.FirstOrDefault()?.Source?.Answer, // nếu có câu trả lời đã lưu của người dùng thì trả về để hiển thị lại, nếu không có thì trả về null
                Options: [.. exercise.Options.Select(o => new ExerciseOption(o.Id, o.Text))],
                AudioUrl: exercise.AudioUrl,
                ImageUrl: exercise.ImageUrl
            );

            return Result<LearningExerciseSessionPracticeDTOResponse>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for exercise session practice item without answer with request: {@SearchRequest}", request);
            return Result<LearningExerciseSessionPracticeDTOResponse>.FailureResult("An error occurred while retrieving the exercise", (int)ErrorCode.INTERNAL_ERROR);
        }
    }
}