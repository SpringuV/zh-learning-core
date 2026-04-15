using FluentValidation;

namespace Lesson.Application.MediatR.Command.Topic.Update;

public class ValidatorUpdateTopic : AbstractValidator<UpdateTopicCommand>
{
    public ValidatorUpdateTopic()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty().WithMessage("Id không được để trống.")
            .Must(id => id != Guid.Empty).WithMessage("Id không hợp lệ.");
        
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự.");
        
        RuleFor(x => x.EstimatedTimeMinutes)
            .GreaterThan(0).WithMessage("Thời gian ước tính phải lớn hơn 0.")
            .LessThanOrEqualTo(1440).WithMessage("Thời gian ước tính không được vượt quá 1440 phút (24 giờ).");
        
        RuleFor(x => x.NewExamYear)
            .GreaterThanOrEqualTo(2000).WithMessage("Năm thi mới phải lớn hơn hoặc bằng 2000.")
            .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage($"Năm thi mới không được vượt quá {DateTime.Now.Year + 1}.");
        
        RuleFor(x => x.NewExamCode)
            .MaximumLength(50).WithMessage("Mã đề thi mới không được vượt quá 50 ký tự.");
    }
}