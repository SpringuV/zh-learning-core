namespace Search.Infrastructure.Queries.Lesson.Search;

public sealed record CountinueLearningExercisesSessionForTopicClientQueries(string Slug, Guid UserId) 
    : IRequest<Result<CountinueLearningResponseDTO>>;

public class CountinueLearningExerciseSessionQueriesHandler(

    ILogger<CountinueLearningExerciseSessionQueriesHandler> logger, 
    ElasticsearchClient client) 
        : IRequestHandler<CountinueLearningExercisesSessionForTopicClientQueries, Result<CountinueLearningResponseDTO>>
{
    private readonly ILogger<CountinueLearningExerciseSessionQueriesHandler> _logger = logger;
    private readonly ElasticsearchClient _client = client;
    public async Task<Result<CountinueLearningResponseDTO>> Handle(CountinueLearningExercisesSessionForTopicClientQueries request, CancellationToken cancellationToken)
    {
        // load cái session item lên
        try
        {
            _logger.LogInformation("Countinue Learning Exercises");
            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.TopicExerciseSessionIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {indexName} does not existReturning empty search result.", ConstantIndexElastic.TopicExerciseSessionIndex);
                return Result<CountinueLearningResponseDTO>.FailureResult("Không có dữ liệu", (int)ErrorCode.NOTFOUND);
            }
            #region load session
            // tìm topic trước để lấy topicId vì session lưu theo topicId, sau đó tìm session mới nhất của userId và topicId đó
            var responseTopic = await _client.SearchAsync<TopicSearch>(s => s
                .Indices(ConstantIndexElastic.TopicIndex)
                .Size(1)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Slug.Suffix("keyword"))
                        .Value(request.Slug))
                ), cancellationToken);
            if (!responseTopic.IsValidResponse || !responseTopic.Documents.Any())
            {
                _logger.LogWarning("Topic with slug {slug} not found in Elasticsearch.", request.Slug);
                return Result<CountinueLearningResponseDTO>.FailureResult("Không tìm thấy chủ đề", (int)ErrorCode.NOTFOUND);
            }
            var topicId = responseTopic.Documents.First().TopicId;
            // sau đó tìm session mới nhất của userId và topicId đó
            var responseExerciseSession = await _client.SearchAsync<TopicExerciseSessionSearch>(s => s
                .Indices(ConstantIndexElastic.TopicExerciseSessionIndex)
                .Size(1) // vì session exercise của 1 người dùng có thể có nhiều session, nhưng sẽ lấy session mới nhất
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.UserId.Suffix("keyword"))
                        .Value(request.UserId.ToString("D")))
                    .Term(t => t
                        .Field(f => f.TopicId.Suffix("keyword"))
                        .Value(topicId.ToString("D"))))
                .Sort(s => s
                    .Field(f => f
                        .Field(ff => ff.InitializedAt)
                        .Order(SortOrder.Desc)
                    )
                ),cancellationToken);
            if (!responseExerciseSession.IsValidResponse)
            {
                _logger.LogError("Invalid Elasticsearch response when loading ExerciseSession");
                return Result<CountinueLearningResponseDTO>.FailureResult("Có lỗi xảy ra khi tải lại bài tập", (int)ErrorCode.INTERNAL_ERROR);
            }
            var documentSession = responseExerciseSession.Documents.First();
            if (documentSession is null)
            {
                _logger.LogInformation("No exercise session found for user {UserId} and topic {TopicId}. Returning not found result.", request.UserId, topicId);
                return Result<CountinueLearningResponseDTO>.FailureResult("Không tìm thấy phiên học nào phù hợp để tiếp tục", (int)ErrorCode.NOTFOUND);
            }
            var currentExerciseItem = documentSession.ExerciseItems.FirstOrDefault(item => item.SequenceNo == documentSession.CurrentSequenceNo)
                ?? documentSession.ExerciseItems.FirstOrDefault();
            if (currentExerciseItem is null)
            {
                _logger.LogWarning("No exercise item found for session {SessionId} in topic {TopicId}.", documentSession.SessionId, topicId);
                return Result<CountinueLearningResponseDTO>.FailureResult("Không tìm thấy câu hỏi để tiếp tục", (int)ErrorCode.NOTFOUND);
            }
            #endregion
            // vì là khi người dùng thoát ra thì be đã lưu lại lịch sử 
            // nên khi load lại thì cứ mặc định là trả ra câu exercise hiện tại trong phiên học
            #region load exercise
            var firstExerciseResponse = await _client.SearchAsync<ExerciseSearch>(s => s
                .Indices(ConstantIndexElastic.ExerciseIndex)
                .Size(1)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.ExerciseId.Suffix("keyword"))
                        .Value(currentExerciseItem.ExerciseId.ToString("D"))
                    )
                ),
                cancellationToken);
            if (!firstExerciseResponse.IsValidResponse || !firstExerciseResponse.Documents.Any())
            {
                _logger.LogInformation("Exercise with ID {ExerciseId} not found in index. Returning not found result.", currentExerciseItem.ExerciseId);
                return Result<CountinueLearningResponseDTO>.FailureResult("Bài tập không còn tồn tại", (int)ErrorCode.NOTFOUND);
            }
            var firstExercise = firstExerciseResponse.Documents.First();
            // load answered của exercise đó nếu có
            var firstExerciseAnswered = await _client.SearchAsync<TopicExerciseAttemptSearch>(s => s
                .Indices(ConstantIndexElastic.TopicExerciseAttemptIndex)
                .Size(1)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            mu => mu.Term(t => t.Field(f => f.ExerciseId.Suffix("keyword")).Value(firstExercise.ExerciseId.ToString("D"))),
                            mu => mu.Term(t => t.Field(f => f.UserId.Suffix("keyword")).Value(request.UserId.ToString("D"))),
                            mu => mu.Term(t => t.Field(f => f.SessionId.Suffix("keyword")).Value(documentSession.SessionId.ToString("D")))
                        )
                    )
                ),
                cancellationToken);
            var firstAnswered = firstExerciseAnswered.Documents.FirstOrDefault()?.Answer;
            #endregion
            return Result<CountinueLearningResponseDTO>.SuccessResult(BuildCountinueLearningResponse(documentSession, firstExercise, firstAnswered));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while continue learning exercises session for topic. Details: {Message}", ex.Message);
            // trả về response mặc định để client có thể hiển thị giao diện tiếp tục học
            return Result<CountinueLearningResponseDTO>.FailureResult("An error occurred while loading continue learning session. Please try again later.", errorCode: (int)ErrorCode.INTERNAL_ERROR);
        }
        throw new NotImplementedException();
    }

    #region Build Response
    private static CountinueLearningResponseDTO BuildCountinueLearningResponse(
        TopicExerciseSessionSearch sessionSearch,
        ExerciseSearch exerciseSearch,
        string? firstAnswered)
    {
        var sessionItems = sessionSearch.ExerciseItems.Select(item =>
            new CountinueLearningSessionItemDTO(
                SessionItemId: item.SessionItemId,
                ExerciseId: item.ExerciseId,
                SequenceNo: item.SequenceNo,
                OrderIndex: item.OrderIndex,
                AttemptId: item.AttemptId,
                Status: item.Status.ToString(),
                ViewedAt: item.ViewedAt,
                AnsweredAt: item.AnsweredAt
            )).ToList();

        var exerciseItem = new CountinueLearningExerciseDTO(
            ExerciseId: exerciseSearch.ExerciseId,
            Description: exerciseSearch.Description,
            Question: exerciseSearch.Question,
            Answerd: firstAnswered, // nếu có câu trả lời đã lưu của người dùng thì trả về để hiển thị lại, nếu không có thì trả về null
            SkillType: exerciseSearch.SkillType.ToString(),
            Difficulty: exerciseSearch.Difficulty.ToString(),
            AudioUrl: exerciseSearch.AudioUrl,
            ImageUrl: exerciseSearch.ImageUrl,
            Options: exerciseSearch.Options
        );

        return new CountinueLearningResponseDTO(
            SessionId: sessionSearch.SessionId,
            TotalExercises: sessionSearch.TotalExercises,
            CurrentSequenceNo: sessionSearch.CurrentSequenceNo,
            SessionItems: sessionItems,
            FirstExercise: exerciseItem
        );
    }
    #endregion
}