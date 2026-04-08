namespace Search.Infrastructure.Queries.Users.Delete;

public record UserDeleteCommand(Guid Id) : IRequest<bool>;
