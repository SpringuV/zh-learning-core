namespace Search.Infrastructure.Queries.Users.Get;

public record UserGetQueries(Guid Id) : IRequest<UserSearchResponse?>;
