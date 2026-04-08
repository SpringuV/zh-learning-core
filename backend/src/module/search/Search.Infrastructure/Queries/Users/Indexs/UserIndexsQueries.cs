namespace Search.Infrastructure.Queries.Users.Indexs;

public record UserIndexsQueries(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt) : IRequest<UserIndexResponse>;
