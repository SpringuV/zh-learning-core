
namespace Auth.Application.Command.Register
{
    // fluent validation cho RegisterUserCommand, sẽ được gọi bởi controller
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email address.");
            RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).WithMessage("Username must be at least 3 characters long.");

            // Thêm regex để check đúng policy của Identity
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
                //.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                //.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                //.Matches("[0-9]").WithMessage("Password must contain at least one number.")
                //.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }
}
