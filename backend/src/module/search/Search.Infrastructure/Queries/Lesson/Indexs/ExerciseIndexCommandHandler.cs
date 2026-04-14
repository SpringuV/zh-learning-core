using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Indexs;
public record ExerciseIndexCommand(
    Guid ExerciseId,
    Guid TopicId,
    string Question,
    string Description,
    int OrderIndex,
    string Slug,
    string CorrectAnswer,
    string ExerciseType,
    string SkillType,
    string Difficulty,
    string Context,
    string? AudioUrl,
    string? ImageUrl,
    string? Explanation,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ExerciseOptionIndexDTOs> Options) : IRequest<ExerciseIndexResponse>;

public class ExerciseIndexCommandHandler(ElasticsearchClient client, ILogger<ExerciseIndexCommandHandler> logger) : IRequestHandler<ExerciseIndexCommand, ExerciseIndexResponse>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<ExerciseIndexCommandHandler> _logger = logger;
    public async Task<ExerciseIndexResponse> Handle(ExerciseIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            #region Validate enum fields
            if(!Enum.TryParse<ExerciseType>(request.ExerciseType, true, out var parsedExerciseType) || !Enum.IsDefined(parsedExerciseType))
            {
                throw new ArgumentException($"ExerciseType '{request.ExerciseType}' không hợp lệ. Giá trị hỗ trợ: {string.Join(", ", Enum.GetNames<ExerciseType>())}.", nameof(request.ExerciseType));
            }
            if(!Enum.TryParse<SkillType>(request.SkillType, true, out var parsedSkillType) || !Enum.IsDefined(parsedSkillType))
            {
                throw new ArgumentException($"SkillType '{request.SkillType}' không hợp lệ. Giá trị hỗ trợ: {string.Join(", ", Enum.GetNames<SkillType>())}.", nameof(request.SkillType));
            }
            if(!Enum.TryParse<ExerciseDifficulty>(request.Difficulty, true, out var parsedDifficulty) || !Enum.IsDefined(parsedDifficulty))
            {
                throw new ArgumentException($"Difficulty '{request.Difficulty}' không hợp lệ. Giá trị hỗ trợ: {string.Join(", ", Enum.GetNames<ExerciseDifficulty>())}.", nameof(request.Difficulty));
            }
            if(!Enum.TryParse<ExerciseContext>(request.Context, true, out var parsedContext) || !Enum.IsDefined(parsedContext))
            {
                throw new ArgumentException($"Context '{request.Context}' không hợp lệ. Giá trị hỗ trợ: {string.Join(", ", Enum.GetNames<ExerciseContext>())}.", nameof(request.Context));
            }
            #endregion

            #region index and check exists
            await EnsureExerciseIndexExistsAsync(cancellationToken);

            var exerciseDocument = new ExerciseSearch(
                exerciseId: request.ExerciseId,
                topicId: request.TopicId,
                question: request.Question,
                description: request.Description,
                orderIndex: request.OrderIndex,
                slug: request.Slug,
                correctAnswer: request.CorrectAnswer,
                exerciseType: parsedExerciseType,
                skillType: parsedSkillType,
                difficulty: parsedDifficulty,
                context: parsedContext,
                audioUrl: request.AudioUrl,
                imageUrl: request.ImageUrl,
                explanation: request.Explanation,
                isPublished: request.IsPublished,
                createdAt: request.CreatedAt,
                updatedAt: request.UpdatedAt,
                options: [..request.Options.Select(o => new ExerciseOption
                (
                    id: o.Id,
                    text: o.Text
                ))]
            );
            var response = await _client.IndexAsync(exerciseDocument, i => i
                    .Index(ConstantIndexElastic.ExerciseIndex)
                    .Id(exerciseDocument.ExerciseId), cancellationToken);
            if (!response.IsValidResponse)            {
                throw new Exception($"Failed to index exercise {request.ExerciseId}: {response.DebugInformation}");
            }
            return new ExerciseIndexResponse(
                ExerciseId: exerciseDocument.ExerciseId,
                CreatedAt: exerciseDocument.CreatedAt
            );
            #endregion
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while indexing exercise");
            throw new NotImplementedException();
        }
    }
    private async Task EnsureExerciseIndexExistsAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.ExerciseIndex, cancellationToken);
        if (!existsResponse.IsValidResponse)
        {
            var createIndexResponse = await _client.Indices.CreateAsync(ConstantIndexElastic.ExerciseIndex, c => c
                .Mappings<ExerciseSearch>(m=> m
                    .Properties(p => p
                        .Text(e => e.ExerciseId, t => t.Fields(f => f.Keyword("keyword"))) // dùng keyword để filter chính xác theo id
                        .Text(e => e.TopicId, t => t.Fields(f => f.Keyword("keyword")))
                        .Text(e => e.Question, t => t.Fields(f => f.Keyword("keyword")))
                        .Text(e => e.Description)
                        .Text(e => e.CorrectAnswer)
                        .Keyword(e => e.ExerciseType)
                        .Keyword(e => e.SkillType)
                        .Keyword(e => e.Difficulty)
                        .Keyword(e => e.Context)
                        .IntegerNumber(e => e.OrderIndex)
                        .Text(e => e.Slug)
                        .Text(e => e.AudioUrl)
                        .Text(e => e.ImageUrl)
                        .Text(e => e.Explanation)
                        .Boolean(e => e.IsPublished)
                        .Date(e => e.CreatedAt)
                        .Date(e => e.UpdatedAt)
                        .Text(e => e.Options)

                    )), cancellationToken);
            if (!createIndexResponse.IsValidResponse) {
                throw new Exception($"Failed to create index {ConstantIndexElastic.ExerciseIndex}: {createIndexResponse.DebugInformation}");
            }
        }
    }
}