namespace Search.Infrastructure.Queries.Users.Search;

public record UserSearchQueries(
    string? Email = null,
    string? Username = null,
    bool? IsActive = null,
    string? PhoneNumber = null,
    int Take = 30,
    int Page = 1,
    UserSortBy SortBy = UserSortBy.CreatedAt,
    bool OrderByDescending = true,
    DateTime? StartCreatedAt = null,
    DateTime? EndCreatedAt = null) : IRequest<SearchQueryResult<UserSearchItemResponse>>;
public class UserSearchQueriesHandler(ILogger<UserSearchQueriesHandler> logger, ElasticsearchClient client) : IRequestHandler<UserSearchQueries, SearchQueryResult<UserSearchItemResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<UserSearchQueriesHandler> _logger = logger;
    public async Task<SearchQueryResult<UserSearchItemResponse>> Handle(UserSearchQueries request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing search with request: {@SearchRequest}", request);

            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.UserIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning empty search result.", ConstantIndexElastic.UserIndex);
                return new SearchQueryResult<UserSearchItemResponse>(
                    Items: [],
                    Pagination: new PaginationResponse(
                        Page: request.Page,
                        PageSize: request.Take,
                        Total: 0));
            }

            var response = await _client.SearchAsync<UserSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.UserIndex);
                s.Size(request.Take);
                if (request.Page > 1)
                {
                    s.From((request.Page - 1) * request.Take);
                }

                s.Query(q => q.Bool(b =>
                {
                    if (!string.IsNullOrWhiteSpace(request.Email))
                    {
                        b.Filter(f => f.Match(m => m.Field(u => u.Email).Query(request.Email)));
                    }

                    if (!string.IsNullOrWhiteSpace(request.Username))
                    {
                        b.Filter(f => f.Match(m => m.Field(u => u.Username).Query(request.Username)));
                    }

                    if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    {
                        b.Filter(f => f.MatchPhrase(m => m.Field(u => u.PhoneNumber).Query(request.PhoneNumber)));
                    }

                    if (request.IsActive.HasValue)
                    {
                        b.Filter(f => f.Term(t => t.Field(u => u.IsActive).Value(request.IsActive.Value)));
                    }

                    if (request.StartCreatedAt.HasValue || request.EndCreatedAt.HasValue)
                    {
                        b.Filter(f => f.Range(r => r.Date(dr =>
                        {
                            dr.Field(u => u.CreatedAt);
                            if (request.StartCreatedAt.HasValue)
                            {
                                dr.Gte(request.StartCreatedAt.Value);
                            }

                            if (request.EndCreatedAt.HasValue)
                            {
                                dr.Lte(request.EndCreatedAt.Value);
                            }

                        })));
                    }

                }));

                var primaryOrder = request.OrderByDescending
                    ? SortOrder.Desc
                    : SortOrder.Asc;

                Action<SortOptionsDescriptor<UserSearch>> primarySort = request.SortBy switch
                {
                    UserSortBy.Email => so => so.Field(u => u.Email.Suffix("keyword"), primaryOrder),
                    UserSortBy.Username => so => so.Field(u => u.Username.Suffix("keyword"), primaryOrder),
                    UserSortBy.UpdatedAt => so => so.Field(u => u.UpdatedAt, primaryOrder),
                    UserSortBy.CurrentLevel => so => so.Field(u => u.CurrentLevel, primaryOrder),
                    _ => so => so.Field(u => u.CreatedAt, primaryOrder)
                };

                s.Sort(
                    primarySort,
                    so => so.Field("id.keyword", SortOrder.Asc));
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to search users: {response.DebugInformation}");
            }

            // Select + ToList mapping immediately
            var results = response.Documents
                .Take(request.Take)
                .Select(user => new UserSearchItemResponse(
                    Id: user.Id,
                    Email: user.Email,
                    Username: user.Username,
                    PhoneNumber: user.PhoneNumber,
                    IsActive: user.IsActive,
                    CreatedAt: user.CreatedAt,
                    UpdatedAt: user.UpdatedAt,
                    CurrentLevel: user.CurrentLevel))
                .ToList();

            var totalHits = response.HitsMetadata?.Total;
            long? totalMatched = null;
            if (totalHits is not null)
            {
                totalMatched = totalHits.Match(
                    hitCount => hitCount?.Value ?? 0,
                    value => value);
            }
            return BuildPagedResult(results, request.Page, request.Take, totalMatched);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchInternalAsync");
            throw;
        }
    }

    private SearchQueryResult<UserSearchItemResponse> BuildPagedResult(
        List<UserSearchItemResponse> results, 
        int page,
        int take, 
        long? totalMatched = null)
    {
        _logger.LogInformation("Query executed successfully: {DocumentCount} results returned", results.Count);

        // totalMatched from response metadata is always accurate (total matching docs)
        var total = totalMatched ?? results.Count;

        return new SearchQueryResult<UserSearchItemResponse>(
            Items: results,
            Pagination: new PaginationResponse(
                Page: page,
                PageSize: take,
                Total: total));
    }

}
