using System.Text.Encodings.Web;

namespace Auth.Application.Interfaces;


public record ValidateUser(string Id, string Username, IReadOnlyList<string> Roles);
public record RegisterResponse(Guid UserId, DateTime CreatedAt, string ActivateCode);
public record ResendMailActivationResponse(string Email, string ActivationCode, DateTime ExpiredActivation);
public interface IIdentityService
{
    // Interface này sẽ được Infrastructure implement bằng UserManager<ApplicationUser>
    // để tận dụng các tính năng xịn sò của ASP.NET Core EF Identity.
    Task<ValidateUser?> ValidateCredentialsAsync(string Username, string Password, string LoginType);
    Task<RegisterResponse?> RegisterUserAsync(string email, string username, string password, CancellationToken cancellationToken = default);
    Task<bool> ChangeUserPasswordAsync(string UserId, string newPassword, string oldPassword, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(string email, string code, CancellationToken cancellationToken = default);
    Task<ResendMailActivationResponse> ResendMailActivateAsync(string email, CancellationToken cancellationToken = default);
}