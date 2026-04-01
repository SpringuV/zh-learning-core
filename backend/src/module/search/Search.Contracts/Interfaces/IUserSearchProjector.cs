namespace Search.Contracts.Interfaces;

public interface IUserSearchProjector
{
    Task IndexAsync(Guid id, string email, string username, bool isActive, string? phoneNumber, DateTime createdAt, DateTime updatedAt, CancellationToken cancellationToken = default);
    Task PatchAsync(string id, UserSearchPatchDocument patch, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public sealed record UserSearchPatchDocument(
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null,
    bool? IsActive = null,
    DateTime? LastLogin = null,
    int? StreakCount = null,
    int? CurrentLevel = null,
    int? TotalExperience = null,
    DateTime? LastActivityAt = null,
    string? AvatarUrl = null,
    DateTime? UpdatedAt = null);