namespace Search.Contracts.DTOs;
public sealed record UserSearchItemResponse(
	string Id,
	string Email,
	string Username,
	string? PhoneNumber,
	bool IsActive,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	int CurrentLevel);