using MediatR;

namespace Auth.Application.Command.ActivateAccount;

public record ActivateAccountCommand(string Email, string Code) : IRequest<bool>;
