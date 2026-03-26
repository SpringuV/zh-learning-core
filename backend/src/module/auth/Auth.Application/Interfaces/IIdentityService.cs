namespace Auth.Application.Interfaces;


public record ValidateUser(string Id, string Username, IReadOnlyList<string> Roles);
public record RegisterResponse(Guid UserId, DateTime CreatedAt, string ActivateCode);
public interface IIdentityService
{
    // Interface này sẽ được Infrastructure implement bằng UserManager<ApplicationUser>
    // để tận dụng các tính năng xịn sò của ASP.NET Core EF Identity.
    Task<ValidateUser?> ValidateCredentialsAsync(string Username, string Password, string LoginType);
    Task<RegisterResponse?> RegisterUserAsync(string email, string username, string password, CancellationToken cancellationToken = default);
    Task<Guid?> ChangeUserPasswordAsync(string? email, string? phoneNumber, string? userName, string newPassword, CancellationToken cancellationToken = default);
}   