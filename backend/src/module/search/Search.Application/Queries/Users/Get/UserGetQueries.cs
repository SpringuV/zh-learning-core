using MediatR;
using Search.Domain.Entities;

namespace Search.Application.Queries.Users.Get;

public record UserGetQueries(Guid Id) : IRequest<UserSearch?>;
