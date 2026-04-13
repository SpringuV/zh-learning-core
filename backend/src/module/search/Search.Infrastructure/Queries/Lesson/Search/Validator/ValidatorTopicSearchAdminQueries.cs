namespace Search.Infrastructure.Queries.Lesson.Search.Validator;

public sealed class TopicSearchAdminQueriesValidator : AbstractValidator<TopicSearchAdminQueries>
{
    public TopicSearchAdminQueriesValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 200)
            .WithMessage("Take must be between 1 and 200.");

        RuleFor(x => x)
            .Must(x => !x.StartCreatedAt.HasValue || !x.EndCreatedAt.HasValue || x.StartCreatedAt <= x.EndCreatedAt)
            .WithMessage("StartCreatedAt must be less than or equal to EndCreatedAt.");

        RuleFor(x => x.TopicType)
            .Must(BeValidTopicType)
            .When(x => !string.IsNullOrWhiteSpace(x.TopicType))
            .WithMessage($"TopicType is invalid. Supported values: {string.Join(", ", Enum.GetNames<TopicType>())}.");

        RuleFor(x => x.SearchAfterValues)
            .Must(v => string.IsNullOrWhiteSpace(v) || SearchAfterCursorHelper.TryParseSearchAfterValues(v, out _))
            .WithMessage("SearchAfterValues format is invalid.");
    }

    private static bool BeValidTopicType(string? topicType)
    {
        if (string.IsNullOrWhiteSpace(topicType))
        {
            return true;
        }

        return Enum.TryParse<TopicType>(topicType, true, out var parsed)
            && Enum.IsDefined(parsed);
    }
}