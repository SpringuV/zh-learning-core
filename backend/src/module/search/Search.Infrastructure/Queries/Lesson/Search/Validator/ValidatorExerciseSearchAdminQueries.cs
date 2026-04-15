using HanziAnhVu.Shared.Domain;

namespace Search.Infrastructure.Queries.Lesson.Search.Validator; 

public sealed class ExerciseSearchAdminQueriesValidator : AbstractValidator<ExerciseSearchAdminQueries>
{
    public ExerciseSearchAdminQueriesValidator()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty()
            .WithMessage("TopicId is required.");
        RuleFor(x => x.Question)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Question))
            .WithMessage("Question is required if provided.");
        RuleFor(x => x.Difficulty)
            .Must(BeValidDifficulty)
            .When(x => !string.IsNullOrWhiteSpace(x.Difficulty))
            .WithMessage($"Difficulty is invalid. Supported values: {string.Join(", ", Enum.GetNames<ExerciseDifficulty>())}.");
        RuleFor(x => x.Context)
            .Must(BeValidContext)
            .When(x => !string.IsNullOrWhiteSpace(x.Context))
            .WithMessage($"Context is invalid. Supported values: {string.Join(", ", Enum.GetNames<ExerciseContext>())}.");
        RuleFor(x => x.SkillType)
            .Must(BeValidSkillType)
            .When(x => !string.IsNullOrWhiteSpace(x.SkillType))
            .WithMessage($"SkillType is invalid. Supported values: {string.Join(", ", Enum.GetNames<SkillType>())}.");
        RuleFor(x => x.ExerciseType)
            .Must(BeValidExerciseType)
            .When(x => !string.IsNullOrWhiteSpace(x.ExerciseType))
            .WithMessage($"ExerciseType is invalid. Supported values: {string.Join(", ", Enum.GetNames<ExerciseType>())}.");
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 200)
            .WithMessage("Take must be between 1 and 200.");

        RuleFor(x => x)
        // Nếu chỉ một trong hai giá trị StartCreatedAt hoặc EndCreatedAt được cung cấp, thì vẫn hợp lệ. 
        // Nếu cả hai đều có giá trị, thì StartCreatedAt phải <= EndCreatedAt.
            .Must(x => !x.StartCreatedAt.HasValue || !x.EndCreatedAt.HasValue || x.StartCreatedAt <= x.EndCreatedAt)
            .WithMessage("StartCreatedAt must be less than or equal to EndCreatedAt.");

        RuleFor(x => x.SearchAfterValues)
            .Must(v => string.IsNullOrWhiteSpace(v) || SearchAfterCursorHelper.TryParseSearchAfterValues(v, out _))
            .WithMessage("SearchAfterValues format is invalid.");
    }
    private static bool BeValidDifficulty(string? difficulty)
    {
        if (string.IsNullOrWhiteSpace(difficulty))
        {
            return true;
        }

        return Enum.TryParse<ExerciseDifficulty>(difficulty, true, out var parsed)
            && Enum.IsDefined(parsed);
    }
    private static bool BeValidExerciseType(string? exerciseType)
    {
        if (string.IsNullOrWhiteSpace(exerciseType))
        {
            return true;
        }

        return Enum.TryParse<ExerciseType>(exerciseType, true, out var parsed)
            && Enum.IsDefined(parsed);
    }

    private static bool BeValidSkillType(string? skillType)
    {
        if (string.IsNullOrWhiteSpace(skillType))
        {
            return true;
        }

        return Enum.TryParse<SkillType>(skillType, true, out var parsed)
            && Enum.IsDefined(parsed);
    }

    private static bool BeValidContext(string? context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return true;
        }

        return Enum.TryParse<ExerciseContext>(context, true, out var parsed)
            && Enum.IsDefined(parsed);
    }
}