
using HanziAnhVu.Shared.Application;
using Search.Contracts.DTOs;
using Search.Domain.Entities;

namespace Search.Contracts.Interfaces;

public interface IUserSearchQueries
{
	Task<SearchQueryResult<UserSearchItem>> SearchUsersAsync(UserSearchQueryRequest request, CancellationToken cancellationToken = default);
    Task<UserSearch?> GetAsync(string id, CancellationToken cancellationToken = default);
}
public enum UserSortBy
{
    CreatedAt,
    UpdatedAt,
    CurrentLevel,
    TotalExperience,
    StreakCount
}
public sealed record UserSearchItem(
	string Id,
	string Email,
	string Username,
	string? PhoneNumber,
	bool IsActive,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	int CurrentLevel,
	int TotalExperience,
	int StreakCount);

