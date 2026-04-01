namespace Search.Domain.Entities;

public record UserSearchDoumentDTOs(Guid Id, string Email, string Username, DateTime CreatedAt, DateTime UpdatedAt, bool IsActive, string? PhoneNumber);
public record UpdateMailUserSearchDTOs(Guid Id, string NewEmail, DateTime UpdatedAt);
public record UpdateAvatarUserSearchDTOs(Guid Id, string? AvatarUrl, DateTime UpdatedAt);
public record UpdatePhoneNumberSearchDTOs(Guid Id, string? PhoneNumber, DateTime UpdatedAt);
public record UpdateUserProgressSearchDTOs(Guid Id, int StreakCount, int CurrentLevel, int TotalExperience, DateTime LastActivityAt, DateTime UpdatedAt);
public record UpdateLastLoginSearchDTOs(Guid Id, DateTime LastLogin, DateTime UpdatedAt);
public class UserSearch
{
    // auth user
    public string Id { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLogin { get; private set; }

    // === Users Domain ===
    public int StreakCount { get; private set; } = 0;
    public int CurrentLevel { get; private set; } = 0;
    public int TotalExperience { get; private set; } = 0;
    public DateTime LastActivityAt { get; private set; }
    public string? AvatarUrl { get; private set; } = null;


    private UserSearch() { }
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
        if (dto.StreakCount < 0) throw new ArgumentException("Streak count cannot be negative");
        if (dto.CurrentLevel < 0) throw new ArgumentException("Current level cannot be negative");
        if (dto.TotalExperience < 0) throw new ArgumentException("Total experience cannot be negative");
        
        StreakCount = dto.StreakCount;
        CurrentLevel = dto.CurrentLevel;
        TotalExperience = dto.TotalExperience;
        LastActivityAt = dto.LastActivityAt;
        UpdatedAt = dto.UpdatedAt;
    }

    public void UpdateLastLogin(UpdateLastLoginSearchDTOs dto)
    {
        LastLogin = dto.LastLogin;
        UpdatedAt = dto.UpdatedAt;
    }
}