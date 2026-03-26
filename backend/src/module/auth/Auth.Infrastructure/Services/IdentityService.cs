
namespace Auth.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AuthApplicationUser> _userManager;

        public IdentityService(UserManager<AuthApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<Guid?> ChangeUserPasswordAsync(string? email, string? phoneNumber, string? userName, string newPassword, CancellationToken cancellationToken = default)
        {
            return ChangeUserPasswordInternalAsync(email, phoneNumber, userName, newPassword, cancellationToken);
        }

        public async Task<RegisterResponse?> RegisterUserAsync(string email, string username, string password, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var user = new AuthApplicationUser
            {
                Id = Guid.CreateVersion7().ToString(),
                Email = email,
                UserName = username,
            };

            var result = await _userManager.CreateAsync(user, password);
            ThrowIfFailed(result, "Failed to register user.");

            if (!Guid.TryParse(user.Id, out var userId))
            {
                throw new AuthDomainException("Created user id is not a valid guid.");
            }

            return new RegisterResponse(Guid.Parse(user.Id), user.CreatedAt, user.ActivateCode);
        }

        private async Task<Guid?> ChangeUserPasswordInternalAsync(
            string? email,
            string? phoneNumber,
            string? userName,
            string newPassword,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

            if (string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(phoneNumber) &&
                string.IsNullOrWhiteSpace(userName))
            {
                throw new AuthDomainException("At least one identifier (email, phone number or username) is required.");
            }

            AuthApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(email))
            {
                user = await _userManager.FindByEmailAsync(email);
            }

            if (user is null && !string.IsNullOrWhiteSpace(userName))
            {
                user = await _userManager.FindByNameAsync(userName);
            }

            if (user is null && !string.IsNullOrWhiteSpace(phoneNumber))
            {
                // n + 1 query, but UserManager does not provide method to find user by phone number,
                // and this method is not used frequently, so it is acceptable
                user = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber);
            }

            if (user is null)
            {
                throw new AuthDomainException("User not found.");
            }

            // check if user can change password (only once every 30 days)
            if (DateTime.UtcNow - user.LastTimeChangePassword < TimeSpan.FromDays(30))
            {
                throw new AuthDomainException("Password can only be changed once every 30 days.");
            }

            var isSamePassword = await _userManager.CheckPasswordAsync(user, newPassword);
            if (isSamePassword)
            {
                throw new AuthDomainException("New password cannot be the same as the old password.");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            ThrowIfFailed(result, "Failed to change user password.");

            if (!Guid.TryParse(user.Id, out var userId))
            {
                throw new AuthDomainException("User id is not a valid guid.");
            }

            return userId;
        }

        private static void ThrowIfFailed(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(x => x.Description));
            throw new AuthDomainException($"{message} {errors}");
        }

        public async Task<ValidateUser?> ValidateCredentialsAsync(string Username, string Password, string LoginType)
        {
            if (string.IsNullOrEmpty(Username))
            { 
                throw new AuthDomainException("Username is required.");
            }
            if (string.IsNullOrEmpty(Password))
            {
                throw new AuthDomainException("Password is required.");
            }
            AuthApplicationUser? user;
            switch (LoginType)
            {
                case "Email":
                    // Handle email login
                    user = await _userManager.FindByEmailAsync(Username);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                    {
                        return null;
                    }
                    var roles = await _userManager.GetRolesAsync(user);
                    return new ValidateUser (user.Id, user.UserName!, roles.ToList());
                case "Phone":
                    // Handle phone login
                    user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == Username);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                    {
                        return null;
                    }
                    return new ValidateUser (user.Id, user.UserName!, new List<string>());
                case "Username":
                    // Handle username login
                    user = await _userManager.FindByNameAsync(Username);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                    {
                        return null;
                    }
                    return new ValidateUser (user.Id, user.UserName!, new List<string>());
                default:
                    throw new AuthDomainException("Invalid login type.");
            }
        }
    }
}
