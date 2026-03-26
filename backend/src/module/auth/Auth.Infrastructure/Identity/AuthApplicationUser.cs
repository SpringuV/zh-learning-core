namespace Auth.Infrastructure.Identity
{
    public class AuthApplicationUser: IdentityUser
    {
        // có thể khai báo thêm các thuộc tính khác nếu cần, ví dụ: FirstName, LastName, v.v.
        public bool IsActive { get; private set; } = false;

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime LastLogin { get; private set; } = DateTime.UtcNow;
        public DateTime LastTimeChangeEmail { get; private set; } = DateTime.UtcNow;
        public DateTime LastTimeChangePassword { get; private set; } = DateTime.UtcNow;
        public ICollection<IdentityUserRole<string>> UserRoles { get; private set; } = new List<IdentityUserRole<string>>();

        public DateTime ExpireTimeActivateCode { get; private set; } = DateTime.UtcNow.AddMinutes(5);
        public string ActivateCode { get; private set; } = Random.Shared.Next(000000, 1000000).ToString("D6");
        public AuthApplicationUser()
        {
            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }

        public void GenerateActivateCode()
        {
            ActivateCode = Random.Shared.Next(000000, 1000000).ToString("D6");
        }

        public void Activate()
        {
            // không cần phải tạo entity mới để active user, vì việc active user
            // chỉ đơn giản là thay đổi trạng thái của user hiện tại, và việc tạo quá nhiều entity có thể làm phức tạp hệ thống

            // việc new entity là ở chỗ handler của UserCreatedDomainEvent, khi mà sau khi tạo user mới thì sẽ tự động active user đó,
            // và việc này là hợp lý vì nó chỉ xảy ra một lần duy nhất khi tạo user mới
            if (IsActive) throw new AuthDomainException("User is already active.");
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive) throw new AuthDomainException("User is already inactive.");
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
            if (DateTime.UtcNow - LastTimeChangePassword < TimeSpan.FromDays(30))
                throw new AuthDomainException("Password can only be changed once every 30 days.");
            if (newPasswordHash == PasswordHash)
                throw new AuthDomainException("New password cannot be the same as the old password.");
            PasswordHash = newPasswordHash;
            LastTimeChangePassword = DateTime.UtcNow;
            UpdatedAt = LastTimeChangePassword;
        }

        public void ChangeEmail(string newEmail)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
            if (DateTime.UtcNow - LastTimeChangeEmail < TimeSpan.FromDays(30))
                throw new AuthDomainException("Email can only be changed once every 30 days.");
            if (newEmail.Equals(Email, StringComparison.OrdinalIgnoreCase)) throw new AuthDomainException("New email cannot be the same as the old email.");
            Email = newEmail;
            LastTimeChangeEmail = DateTime.UtcNow;
            UpdatedAt = LastTimeChangeEmail;
        }
        public void UpdateLastLogin()
        {
            LastLogin = DateTime.UtcNow;
            UpdatedAt = LastLogin;
        }
    }
}
