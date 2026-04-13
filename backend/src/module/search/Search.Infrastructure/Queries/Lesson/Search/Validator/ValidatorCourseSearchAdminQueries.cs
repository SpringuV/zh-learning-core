namespace Search.Infrastructure.Queries.Lesson.Search.Validator;


public sealed class CourseSearchAdminQueriesValidator : AbstractValidator<CourseSearchAdminQueries>
{
    public CourseSearchAdminQueriesValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 200)
            .WithMessage("Take must be between 1 and 200.");

        RuleFor(x => x)
        // Nếu chỉ một trong hai giá trị StartCreatedAt hoặc EndCreatedAt được cung cấp, thì vẫn hợp lệ. 
        // Nếu cả hai đều có giá trị, thì StartCreatedAt phải nhỏ hơn hoặc bằng EndCreatedAt.
            .Must(x => !x.StartCreatedAt.HasValue || !x.EndCreatedAt.HasValue || x.StartCreatedAt <= x.EndCreatedAt)
            .WithMessage("StartCreatedAt must be less than or equal to EndCreatedAt.");

        RuleFor(x => x.SearchAfterValues)
            .Must(v => string.IsNullOrWhiteSpace(v) || SearchAfterCursorHelper.TryParseSearchAfterValues(v, out _))
            .WithMessage("SearchAfterValues format is invalid.");
    }
}