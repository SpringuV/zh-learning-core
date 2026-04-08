namespace Search.Application.Entities;

public class UserSearch
{
    // auth user
    public string Id { get; set; } = string.Empty;
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
        string Id,
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
        ValidateEmail(email);
        
        return new UserSearch
        {
            Id = id.ToString(),
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
        ValidateEmail(newEmail);
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
        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length < 10)
            throw new ArgumentException("Số điện thoại không hợp lệ. Số điện thoại phải có ít nhất 10 ký tự.", nameof(phoneNumber));
        
        PhoneNumber = phoneNumber;
        UpdatedAt = updatedAt;
    }

    public void UpdateUserProgress(int currentLevel, DateTime lastActivityAt, DateTime updatedAt)
    {
        if (currentLevel < 0)
            throw new ArgumentException("Cấp độ hiện tại không thể là số âm.", nameof(currentLevel));
        
        CurrentLevel = currentLevel;
        UpdatedAt = updatedAt;
    }

    public void UpdateLastLogin(DateTime lastLogin, DateTime updatedAt)
    {
        LastLogin = lastLogin;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không thể để trống.", nameof(email));
        
        if (!email.Contains("@"))
            throw new ArgumentException("Định dạng email không hợp lệ.", nameof(email));
    }
}