namespace Auth.Application.Command.Login;

public record LoginUserCommand(string Username, string Password, string TypeLogin) : IRequest<TokenResult?>;
