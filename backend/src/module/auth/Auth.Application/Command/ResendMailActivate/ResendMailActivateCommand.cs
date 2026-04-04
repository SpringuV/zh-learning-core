namespace Auth.Application.Command.ResendMail;

public record ResendMailActivateCommand(string Account, string TypeUsername, CancellationToken ct) : IRequest<bool>;
