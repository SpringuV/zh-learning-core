namespace Lesson.Application.MediatR.Command.Course.Create;

public class ValidatorCreate : AbstractValidator<CreateCourseCommand>
{
    public ValidatorCreate()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả không được để trống.")
            .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự.");

        RuleFor(x => x.HskLevel)
            .InclusiveBetween(1, 9).WithMessage("HskLevel phải từ 1 đến 9.");
        
    }
}