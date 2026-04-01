using HanziAnhVu.Shared.Application;
using MediatR;
using Search.Contracts.Interfaces;

namespace Search.Application.Queries.Users;

public class UserSearchQueriesHandler : IRequestHandler<UserSearchQueries, SearchQueryResult<UserSearchItem>>
{
    public Task<SearchQueryResult<UserSearchItem>> Handle(UserSearchQueries request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
