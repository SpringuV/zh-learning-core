namespace Auth.Application.Interfaces;

public interface IIdentityService
{
    // Interface này sẽ được Infrastructure implement bằng UserManager<ApplicationUser>
    // để tận dụng các tính năng xịn sò của ASP.NET Core EF Identity.
    Task<Guid> RegisterUserAsync(string email, string username, string password, CancellationToken cancellationToken = default);
    Task<Guid> ChangeUserPasswordAsync(string? email, string? phoneNumber, string? userName, string newPassword, CancellationToken cancellationToken = default);
}   