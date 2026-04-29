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
            .When(x => x.Difficulty.HasValue)
            .WithMessage($"Difficulty is invalid. Supported values: {string.Join(", ", Enum.GetNames<ExerciseDifficulty>())}.");
        RuleFor(x => x.Context)
            .Must(BeValidContext)
            .When(x => x.Context.HasValue)
            .WithMessage($"Context is invalid. Supported values: {string.Join(", ", Enum.GetNames<ExerciseContext>())}.");
        RuleFor(x => x.SkillType)
            .Must(BeValidSkillType)
            .When(x => x.SkillType.HasValue)
            .WithMessage($"SkillType is invalid. Supported values: {string.Join(", ", Enum.GetNames<SkillType>())}.");
        RuleFor(x => x.ExerciseType)
            .Must(BeValidExerciseType)
            .When(x => x.ExerciseType.HasValue)
            .WithMessage($"ExerciseType is invalid. Supported values: {string.Join(", ", Enum.GetNames<ExerciseType>())}.");
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 200)
            .WithMessage("Take must be between 1 and 200.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x)
        // Nếu chỉ một trong hai giá trị StartCreatedAt hoặc EndCreatedAt được cung cấp, thì vẫn hợp lệ. 
        // Nếu cả hai đều có giá trị, thì StartCreatedAt phải <= EndCreatedAt.
            .Must(x => !x.StartCreatedAt.HasValue || !x.EndCreatedAt.HasValue || x.StartCreatedAt <= x.EndCreatedAt)
            .WithMessage("StartCreatedAt must be less than or equal to EndCreatedAt.");
    }
    private static bool BeValidDifficulty(ExerciseDifficulty? difficulty)
    {
        return !difficulty.HasValue || Enum.IsDefined(difficulty.Value);
    }
    private static bool BeValidExerciseType(ExerciseType? exerciseType)
    {
        return !exerciseType.HasValue || Enum.IsDefined(exerciseType.Value);
    }

    private static bool BeValidSkillType(SkillType? skillType)
    {
        return !skillType.HasValue || Enum.IsDefined(skillType.Value);
    }

    private static bool BeValidContext(ExerciseContext? context)
    {
        return !context.HasValue || Enum.IsDefined(context.Value);
    }
}