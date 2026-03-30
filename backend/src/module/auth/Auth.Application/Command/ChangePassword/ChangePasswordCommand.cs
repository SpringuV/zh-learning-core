namespace Auth.Application.Command.ChangePassword;

public record ChangePasswordCommand(string UserId, string OldPassword, string NewPassword): IRequest<bool>;
