namespace Auth.Application.Command.UpdateProfile;

public record UpdateProfileCommand(Guid UserId, string? NewPhoneNumber, string? NewAvatarUrl) : IRequest<bool>;