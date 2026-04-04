namespace Search.Domain.Entities;

public record UserSearchDoumentDTOs(Guid Id, string Email, string Username, DateTime CreatedAt, DateTime UpdatedAt, bool IsActive, string? PhoneNumber);
public record UpdateMailUserSearchDTOs(Guid Id, string NewEmail, DateTime UpdatedAt);
public record UpdateAvatarUserSearchDTOs(Guid Id, string? AvatarUrl, DateTime UpdatedAt);
public record UpdatePhoneNumberSearchDTOs(Guid Id, string? PhoneNumber, DateTime UpdatedAt);
public record UpdateUserProgressSearchDTOs(Guid Id, int CurrentLevel, int TotalExperience, DateTime LastActivityAt, DateTime UpdatedAt);
public record UpdateLastLoginSearchDTOs(Guid Id, DateTime LastLogin, DateTime UpdatedAt);
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

    // === Users Domain ===
    public int CurrentLevel { get; set; } = 0;
    public DateTime LastActivityAt { get; set; }
    public string? AvatarUrl { get; set; } = null;


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
        DateTime? LastActivityAt = null,
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
        this.LastActivityAt = LastActivityAt ?? DateTime.MinValue;
        this.AvatarUrl = AvatarUrl;
    }

    public static UserSearch FromUser(UserSearchDoumentDTOs dto)
    {
        return new UserSearch
        {
            Id = dto.Id.ToString(),
            Email = dto.Email,
            Username = dto.Username,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsActive = dto.IsActive
        };
    }   

    public void UpdateEmail(UpdateMailUserSearchDTOs dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewEmail)) 
            throw new ArgumentException("Email cannot be empty");
        if (!dto.NewEmail.Contains("@")) 
            throw new ArgumentException("Invalid email format");
        Email = dto.NewEmail;
        UpdatedAt = dto.UpdatedAt;
    }


    public void UpdateAvatarUrl(UpdateAvatarUserSearchDTOs dto)
    {
        AvatarUrl = dto.AvatarUrl;
        UpdatedAt = dto.UpdatedAt;
    }

    public void UpdatePhoneNumber(UpdatePhoneNumberSearchDTOs dto)
    {
        if (!string.IsNullOrEmpty(dto.PhoneNumber) && dto.PhoneNumber.Length < 10)
            throw new ArgumentException("Invalid phone number");
        PhoneNumber = dto.PhoneNumber;
        UpdatedAt = dto.UpdatedAt;
    }

    public void UpdateUserProgress(UpdateUserProgressSearchDTOs dto)
    {
        if (dto.CurrentLevel < 0) throw new ArgumentException("Current level cannot be negative");
        
        CurrentLevel = dto.CurrentLevel;
        LastActivityAt = dto.LastActivityAt;
        UpdatedAt = dto.UpdatedAt;
    }

    public void UpdateLastLogin(UpdateLastLoginSearchDTOs dto)
    {
        LastLogin = dto.LastLogin;
        UpdatedAt = dto.UpdatedAt;
    }
}