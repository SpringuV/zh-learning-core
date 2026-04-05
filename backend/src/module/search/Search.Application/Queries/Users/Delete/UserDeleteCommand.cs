using MediatR;

namespace Search.Application.Queries.Users.Delete;

public record UserDeleteCommand(Guid Id) : IRequest<bool>;
