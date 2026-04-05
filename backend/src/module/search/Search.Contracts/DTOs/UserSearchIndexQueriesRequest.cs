namespace Search.Contracts.DTOs;

public record UserSearchIndexQueriesRequest(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt);
