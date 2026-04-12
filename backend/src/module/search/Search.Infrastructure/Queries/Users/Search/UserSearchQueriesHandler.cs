namespace Search.Infrastructure.Queries.Users.Search;

public record UserSearchQueries(
    string? Email = null,
    string? Username = null,
    bool? IsActive = null,
    string? PhoneNumber = null,
    int Take = 30,
    // Keyset pagination: JSON array của [sortValue, userId]
    // Ví dụ: "[5, \"user-123\"]" khi sort by CurrentLevel
    // Ensures stable pagination khi sort field thay đổi
    string? SearchAfterValues = null,
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

                // Important: emit two sort options to match search_after [sortValue, userId].
                s.Sort(
                    primarySort,
                    so => so.Field("id.keyword", SortOrder.Asc));

                // Keyset pagination: SearchAfter [sortValue, userId]
                if (!string.IsNullOrWhiteSpace(request.SearchAfterValues))
                {
                    if (SearchAfterCursorHelper.TryParseSearchAfterValues(request.SearchAfterValues, out var fieldValues))
                    {
                        s.SearchAfter(fieldValues);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid SearchAfterValues format: {SearchAfterValues}", request.SearchAfterValues);
                    }
                }
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to search users: {response.DebugInformation}");
            }

            // Select + ToList mapping immediately
            var results = response.Documents
                .Take(request.Take + 1) // Fetch one extra for pagination check
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
            return BuildPagedResult(results, request.Take, totalMatched, request.SortBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchInternalAsync");
            throw;
        }
    }

    private SearchQueryResult<UserSearchItemResponse> BuildPagedResult(
        List<UserSearchItemResponse> results, 
        int take, 
        long? totalMatched = null, 
        UserSortBy sortBy = UserSortBy.CreatedAt)
    {
        _logger.LogInformation("Query executed successfully: {DocumentCount} results returned", results.Count);

        // hasNextPage: check if we have more docs than take (We fetched take + 1)
        var hasNextPage = results.Count > take;
        if (hasNextPage)
        {
            // Trim sentinel document in-place to avoid allocating an extra list.
            results.RemoveAt(results.Count - 1);
        }

        // totalMatched from response metadata is always accurate (total matching docs)
        var total = totalMatched ?? results.Count;

        var nextCursor = string.Empty;
        if (hasNextPage && results.Count > 0)
        {
            // Get lastDoc from original documents for accurate sort value
            var lastDoc = results[^1];
            var sortValue = GetSortValue(lastDoc, sortBy);
            var cursorJson = SearchAfterCursorHelper.BuildCursor(sortValue, lastDoc.Id);
            nextCursor = cursorJson;
        }

        return new SearchQueryResult<UserSearchItemResponse>(
            Total: total,
            Items: results,
            HasNextPage: hasNextPage,
            NextCursor: nextCursor);
    }

    private static object GetSortValue(UserSearchItemResponse user, UserSortBy sortBy)
    {
        return sortBy switch
        {
            UserSortBy.Email => user.Email,
            UserSortBy.Username => user.Username,
            UserSortBy.UpdatedAt => user.UpdatedAt,
            UserSortBy.CurrentLevel => user.CurrentLevel,
            _ => user.CreatedAt
        };
    }

}
