namespace Auth.Application.Command.Refresh;

public record RefreshTokenCommand(string RefreshToken): IRequest<TokenResult?>;
