using Auth.Domain.Events;
using Auth.Domain.Exceptions;
using HanziAnhVu.Shared.Domain;

namespace HanziAnhVu.Modules.Auth.Domain;
// Auth.Domain (Sạch, không dính dáng framework)
public class UserAggregate : BaseAggregateRoot
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty; // private set để tránh việc thay đổi email sau khi đã tạo user
    public string UserName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = false;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public DateTime LastLogin { get; private set; } = DateTime.UtcNow;
    public DateTime LastTimeChangeEmail { get; private set; } = DateTime.UtcNow;
    public DateTime LastTimeChangePassword { get; private set; } = DateTime.UtcNow;

    protected UserAggregate() { } // constructor protected để chỉ cho phép tạo user thông qua factory method

    // factory method để tạo user mới, đảm bảo rằng tất cả các trường cần thiết được khởi tạo đúng cách
    // để static để có thể gọi mà không cần instance của UserAggregate, và trả về một instance mới của UserAggregate
    public static UserAggregate Create(string email, string username, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new UserAggregate
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        // Fire domain event — các handler khác lắng nghe
        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, user.Email));

        return user;
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
        AddDomainEvent(new UserActivatedDomainEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) throw new AuthDomainException("User is already inactive.");
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeactivatedDomainEvent(Id));
    }

    public void ChangePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        if (DateTime.UtcNow - LastTimeChangePassword < TimeSpan.FromDays(30))
            throw new AuthDomainException("Password can only be changed once every 30 days.");
        if (newPasswordHash == PasswordHash) 
            throw new AuthDomainException("New password cannot be the same as the old password.");
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserPasswordChangedDomainEvent(Id));
    }

    public void ChangeEmail(string newEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
        if (DateTime.UtcNow - LastTimeChangeEmail < TimeSpan.FromDays(30))
                throw new AuthDomainException("Email can only be changed once every 30 days.");
        if (newEmail.Equals(Email, StringComparison.OrdinalIgnoreCase)) throw new AuthDomainException("New email cannot be the same as the old email.");
        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserEmailChangedDomainEvent(Id, newEmail));
    }
     public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

}
