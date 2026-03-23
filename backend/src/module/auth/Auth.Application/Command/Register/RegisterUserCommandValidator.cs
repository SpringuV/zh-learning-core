
namespace Auth.Application.Command.Register
{
    // fluent validation cho RegisterUserCommand, sẽ được gọi bởi controller
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Invalid email address.");
            RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).WithMessage("Username must be at least 3 characters long.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }
}
