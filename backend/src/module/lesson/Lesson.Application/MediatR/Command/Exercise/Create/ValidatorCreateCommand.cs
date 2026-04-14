using FluentValidation;

namespace Lesson.Application.MediatR.Command.Exercise.Create;

public sealed class ValidatorCreateCommand : AbstractValidator<CreateExerciseCommand>
{
    public ValidatorCreateCommand()
    {
        RuleFor(x => x.TopicId).NotEmpty().WithMessage("TopicId is required.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.SkillType).IsInEnum().WithMessage("SkillType is invalid.");
        RuleFor(x => x.ExerciseContext).IsInEnum().WithMessage("ExerciseContext is invalid.");
        RuleFor(x => x.ExerciseType).IsInEnum().WithMessage("ExerciseType is invalid.");
        RuleFor(x => x.Difficulty).IsInEnum().WithMessage("Difficulty is invalid.");
        RuleFor(x => x.Question).NotEmpty().WithMessage("Question is required.");
        RuleFor(x => x.CorrectAnswer).NotEmpty().WithMessage("CorrectAnswer is required.");

        // Audio/Image là optional: chỉ validate format khi client có gửi giá trị.
        RuleFor(x => x.AudioUrl)
            .Must(BeValidAbsoluteUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.AudioUrl))
            .WithMessage("AudioUrl must be a valid absolute URL when provided.");

        // ImageUrl có thể dùng để lưu URL ảnh cho các bài tập dạng ListenImageChoice, 
        // nên cũng cần validate nếu client có gửi.
        RuleFor(x => x.ImageUrl)
            .Must(BeValidAbsoluteUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("ImageUrl must be a valid absolute URL when provided.");

        // Với exercise type cần options: bắt buộc có options và đáp án đúng phải tồn tại trong option ids.
        When(x => ExerciseAggregate.RequiresOptions(x.ExerciseType), () =>
        {
            RuleFor(x => x.Options)
                .NotNull().WithMessage("Options are required for this exercise type.")
                .Must(options => options is { Count: >= 2 })
                .WithMessage("At least 2 options are required for this exercise type.");

            // Đảm bảo CorrectAnswer khớp với một trong các option id để tránh dữ liệu
            // không nhất quán.
            RuleFor(x => x.CorrectAnswer)
                .Must((command, correctAnswer) =>
                    command.Options is not null &&
                    command.Options.Any(o => o.Id == correctAnswer))
                .WithMessage("CorrectAnswer must match one of the option ids.");
        });

        // Với exercise type không cần options: cho phép null/empty, nhưng nếu có dữ liệu options thì reject để tránh payload mơ hồ.
        When(x => !ExerciseAggregate.RequiresOptions(x.ExerciseType), () =>
        {
            RuleFor(x => x.Options)
                .Must(options => options is null || options.Count == 0)
                .WithMessage("Options must be null or empty for this exercise type.");
        });
    }

    private static bool BeValidAbsoluteUrl(string? value)
    {
        // Uri.TryCreate sẽ kiểm tra xem value có phải là URL hợp lệ hay không, 
        // và UriKind.Absolute đảm bảo rằng URL phải có scheme (http, https, etc.) 
        // và host.
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}