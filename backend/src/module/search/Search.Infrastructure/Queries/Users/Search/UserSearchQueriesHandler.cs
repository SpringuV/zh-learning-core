
namespace Search.Infrastructure.Queries.Users.Search;

public class UserSearchQueriesHandler(ILogger<UserSearchQueriesHandler> logger, ElasticsearchClient client) : IRequestHandler<UserSearchQueries, SearchQueryResult<UserSearchItemResponse>>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<UserSearchQueriesHandler> _logger = logger;
    public async Task<SearchQueryResult<UserSearchItemResponse>> Handle(UserSearchQueries request, CancellationToken cancellationToken)
    {
        var searchResult = await SearchInternalAsync(
            request,
            cancellationToken: cancellationToken);

        var items = searchResult.Items
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

        return new SearchQueryResult<UserSearchItemResponse>(
            Total: searchResult.Total,
            Items: items,
            HasNextPage: searchResult.HasNextPage,
            NextCursor: searchResult.NextCursor);
    }

    private async Task<SearchQueryResult<UserSearch>> SearchInternalAsync(
        UserSearchQueries request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Executing search with request: {@SearchRequest}", request);

            var indexExistsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.UserIndex, cancellationToken);
            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} does not exist. Returning empty search result.", ConstantIndexElastic.UserIndex);
                return new SearchQueryResult<UserSearch>(
                    Total: 0,
                    Items: [],
                    HasNextPage: false,
                    NextCursor: string.Empty);
            }

            var response = await _client.SearchAsync<UserSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.UserIndex);
                s.Size(request.Take + 1);

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

                    if (!string.IsNullOrWhiteSpace(request.SearchAfterCreatedAt) && DateTime.TryParse(request.SearchAfterCreatedAt, out var searchAfterCreatedAt))
                    {
                        b.Filter(f => f.Range(r => r.Date(dr =>
                        {
                            dr.Field(u => u.CreatedAt);
                            if (request.OrderByDescending)
                            {
                                dr.Lt(searchAfterCreatedAt);
                            }
                            else
                            {
                                dr.Gt(searchAfterCreatedAt);
                            }

                        })));
                    }
                }));

                s.Sort(sort =>
                {
                    switch (request.SortBy)
                    {
                        case UserSortBy.Email:
                            sort.Field(f => f.Field(u => u.Email).Order(request.OrderByDescending
                                ? SortOrder.Desc
                                : SortOrder.Asc));
                            break;
                        case UserSortBy.Username:
                            sort.Field(f => f.Field(u => u.Username).Order(request.OrderByDescending
                                ? SortOrder.Desc
                                : SortOrder.Asc));
                            break;
                        case UserSortBy.UpdatedAt:
                            sort.Field(f => f.Field(u => u.UpdatedAt).Order(request.OrderByDescending
                                ? SortOrder.Desc
                                : SortOrder.Asc));
                            break;
                        case UserSortBy.CurrentLevel:
                            sort.Field(f => f.Field(u => u.CurrentLevel).Order(request.OrderByDescending
                                ? SortOrder.Desc
                                : SortOrder.Asc));
                            break;
                        default:
                            sort.Field(f => f.Field(u => u.CreatedAt).Order(request.OrderByDescending
                                ? SortOrder.Desc
                                : SortOrder.Asc));
                            break;
                    }
                });
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to search users: {response.DebugInformation}");
            }

            var documents = response.Documents.ToList();
            var totalHits = response.HitsMetadata?.Total;
            long? totalMatched = null;
            if (totalHits is not null)
            {
                totalMatched = totalHits.Match(
                    hitCount => hitCount?.Value ?? 0,
                    value => value);
            }
            return BuildPagedResult(documents, request.Take, totalMatched);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchInternalAsync");
            throw;
        }
    }

    private SearchQueryResult<UserSearch> BuildPagedResult(List<UserSearch> documents, int take, long? totalMatched = null)
    {
        _logger.LogInformation("Query executed successfully: {DocumentCount} documents returned", documents.Count);

        var hasNextPage = documents.Count > take;
        var results = documents.Take(take).ToList();
        var total = totalMatched ?? results.Count;

        return new SearchQueryResult<UserSearch>(
            Total: total,
            Items: results,
            HasNextPage: hasNextPage,
            NextCursor: hasNextPage && results.Count > 0 ? results.Last().CreatedAt.ToString("O") : string.Empty);
    }

}
