namespace Lesson.Application.MediatR.Command.Exercise.Update;

public class ValidatorUpdateExercise : AbstractValidator<UpdateExerciseCommand>
{
    public ValidatorUpdateExercise()
    {
        RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("ExerciseId is required.");
        RuleFor(x => x.Description).MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
        RuleFor(x => x.Question).MaximumLength(1000).WithMessage("Question cannot exceed 1000 characters.");
        RuleFor(x => x.CorrectAnswer).MaximumLength(500).WithMessage("CorrectAnswer cannot exceed 500 characters.");
        RuleFor(x => x.ExerciseType).IsInEnum().WithMessage("Invalid ExerciseType value.");
        RuleFor(x => x.SkillType).IsInEnum().WithMessage("Invalid SkillType value.");
        RuleFor(x => x.Difficulty).IsInEnum().WithMessage("Invalid Difficulty value.");
        RuleFor(x => x.ExerciseContext).IsInEnum().WithMessage("Invalid ExerciseContext value.");
        RuleFor(x => x.AudioUrl).MaximumLength(500).WithMessage("AudioUrl cannot exceed 500 characters.");
        RuleFor(x => x.ImageUrl).MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters.");
        RuleFor(x => x.Explanation).MaximumLength(2000).WithMessage("Explanation cannot exceed 2000 characters.");
    }
}