namespace Search.Application.Entities;

public class UserSearch
{
    // auth user
    public Guid Id { get; set; } = Guid.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? AvatarUrl { get; set; } = null;
    
    // === Users Domain ===
    public int CurrentLevel { get; set; } = 0;
    


    public UserSearch() { }

    public UserSearch(
        Guid Id,
        string Email,
        string Username,
        string? PhoneNumber,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool IsActive,
        DateTime? LastLogin = null,
        int CurrentLevel = 0,
        string? AvatarUrl = null)
    {
        this.Id = Id;
        this.Email = Email;
        this.Username = Username;
        this.PhoneNumber = PhoneNumber;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
        this.IsActive = IsActive;
        this.LastLogin = LastLogin;
        this.CurrentLevel = CurrentLevel;
        this.AvatarUrl = AvatarUrl;
    }

    public static UserSearch FromUser(
        Guid id,
        string email,
        string username,
        string? phoneNumber,
        DateTime createdAt,
        DateTime updatedAt,
        bool isActive)
    {
        return new UserSearch
        {
            Id = id,
            Email = email,
            Username = username,
            PhoneNumber = phoneNumber,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsActive = isActive
        };
    }   

    public void UpdateEmail(string newEmail, DateTime updatedAt)
    {
        Email = newEmail;
        UpdatedAt = updatedAt;
    }


    public void UpdateAvatarUrl(string? avatarUrl, DateTime updatedAt)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = updatedAt;
    }

    public void UpdatePhoneNumber(string? phoneNumber, DateTime updatedAt)
    {
        PhoneNumber = phoneNumber;
        UpdatedAt = updatedAt;
    }

    public void UpdateUserProgress(int currentLevel, DateTime lastActivityAt, DateTime updatedAt)
    {
        CurrentLevel = currentLevel;
        UpdatedAt = updatedAt;
    }

    public void UpdateLastLogin(DateTime lastLogin, DateTime updatedAt)
    {
        LastLogin = lastLogin;
        UpdatedAt = updatedAt;
    }
}