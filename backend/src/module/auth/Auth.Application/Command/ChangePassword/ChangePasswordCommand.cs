namespace Auth.Application.Command.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string OldPassword, string NewPassword): IRequest<bool>;
