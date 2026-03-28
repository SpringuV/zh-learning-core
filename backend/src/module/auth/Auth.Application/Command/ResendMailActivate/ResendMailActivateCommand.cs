namespace Auth.Application.Command.ResendMail;

public record ResendMailActivateCommand(string Email, CancellationToken ct) : IRequest<bool>;
