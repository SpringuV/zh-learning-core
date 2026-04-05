using MediatR;

namespace Search.Application.Queries.Users.Delete;

public record UserDeleteCommand(string Id) : IRequest<bool>;
