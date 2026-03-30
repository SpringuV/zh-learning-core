namespace Auth.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AuthApplicationUser> _userManager;

        public IdentityService(UserManager<AuthApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> ChangeUserPasswordAsync(string UserId, string newPassword, string oldPassword, CancellationToken cancellationToken = default)
        {
            return await ChangeUserPasswordInternalAsync(UserId, newPassword, oldPassword, cancellationToken);
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
            await _userManager.AddToRoleAsync(user, Constants.USERS);
            ThrowIfFailed(result, "Failed to register user.");

            if (Guid.TryParse(user.Id, result: out Guid userId))
            {
                return new RegisterResponse(userId, user.CreatedAt, user.ActivateCode);
            }

            throw new AuthDomainException("Created user id is not a valid guid.");
        }

        public async Task<bool> ActivateUserAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                throw new AuthDomainException("User not found.");
            }

            if (user.IsActive)
            {
                throw new AuthDomainException("User is already active.");
            }

            if (user.ActivateCode != code)
            {
                throw new AuthDomainException("Invalid activation code.");
            }

            if (user.ExpireTimeActivateCode < DateTime.UtcNow)
            {
                throw new AuthDomainException("Activation code has expired.");
            }

            user.Activate();
            var result = await _userManager.UpdateAsync(user);
            ThrowIfFailed(result, "Failed to activate user.");

            return true;
        }

        #region function ChangePasswordInternal
        private async Task<bool> ChangeUserPasswordInternalAsync(
            string userId,
            string newPassword,
            string oldPassword,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new AuthDomainException("User id is required.");
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new AuthDomainException("New password is required.");
            } 
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                throw new AuthDomainException("Old password is required.");
            }
            if (!string.IsNullOrWhiteSpace(oldPassword))
            {
                if (newPassword == oldPassword)
                {
                    throw new AuthDomainException("New password cannot be the same as the old password.");
                }
            }
            AuthApplicationUser? user = null;

            if (user is null && !string.IsNullOrWhiteSpace(newPassword) && !string.IsNullOrWhiteSpace(oldPassword))
            {
                // n + 1 query, but UserManager does not provide method to find user by phone number,
                // and this method is not used frequently, so it is acceptable
                user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
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

            return true;
        }
        #endregion

        private static void ThrowIfFailed(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(x => x.Description));
            throw new AuthDomainException($"{message} {errors}");
        }

        #region Validate Credential
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
            IList<string> roles;
            switch (LoginType)
            {
                case "Email":
                    // Handle email login
                    user = await _userManager.FindByEmailAsync(Username);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                    {
                        return null;
                    }
                    roles = await _userManager.GetRolesAsync(user);
                    return new ValidateUser (user.Id, user.UserName!, [.. roles]); // C# 14 collection expression
                case "Phone":
                    // Handle phone login
                    user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == Username);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                    {
                        return null;
                    }
                    roles = await _userManager.GetRolesAsync(user);
                    return new ValidateUser (user.Id, user.UserName!, [.. roles]);
                case "Username":
                    // Handle username login
                    user = await _userManager.FindByNameAsync(Username);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, Password))
                    {
                        return null;
                    }
                    roles = await _userManager.GetRolesAsync(user);
                    return new ValidateUser (user.Id, user.UserName!, [.. roles]);
                default:
                    throw new AuthDomainException("Invalid login type.");
            }
        }
        #endregion
        public async Task<ResendMailActivationResponse> ResendMailActivateAsync(string email, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                throw new AuthDomainException("User not found.");
            }

            if (user.IsActive)
            {
                throw new AuthDomainException("User is already active.");
            }

            user.ActivateCode = Guid.CreateVersion7().ToString();
            user.ExpireTimeActivateCode = DateTime.UtcNow.AddMinutes(5);
            var result = await _userManager.UpdateAsync(user);
            ThrowIfFailed(result, "Lỗi khi cập nhật thông tin người dùng - resend mail activate.");
            return new ResendMailActivationResponse(user.Email!, user.ActivateCode, user.ExpireTimeActivateCode);
        }
    }
}
