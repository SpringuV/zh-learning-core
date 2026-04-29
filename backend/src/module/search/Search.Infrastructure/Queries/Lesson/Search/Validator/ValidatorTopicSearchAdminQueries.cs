namespace Search.Infrastructure.Queries.Lesson.Search.Validator;

public sealed class TopicSearchAdminQueriesValidator : AbstractValidator<TopicSearchAdminQueries>
{
    public TopicSearchAdminQueriesValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 200)
            .WithMessage("Take must be between 1 and 200.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x)
            .Must(x => !x.StartCreatedAt.HasValue || !x.EndCreatedAt.HasValue || x.StartCreatedAt <= x.EndCreatedAt)
            .WithMessage("StartCreatedAt must be less than or equal to EndCreatedAt.");

        RuleFor(x => x.TopicType)
            .Must(BeValidTopicType)
            .When(x => x.TopicType.HasValue)
            .WithMessage($"TopicType is invalid. Supported values: {string.Join(", ", Enum.GetNames<TopicType>())}.");
    }

    private static bool BeValidTopicType(TopicType? topicType)
    {
        return !topicType.HasValue || Enum.IsDefined(topicType.Value);
    }
}