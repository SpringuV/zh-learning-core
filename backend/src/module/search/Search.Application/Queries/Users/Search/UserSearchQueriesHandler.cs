using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Esql.Extensions;
using HanziAnhVu.Shared.Application;
using MediatR;
using Microsoft.Extensions.Logging;
using Search.Contracts.DTOs;
using Search.Contracts.Interfaces;
using Search.Domain.Entities;

namespace Search.Application.Queries.Users.Search;

public class UserSearchQueriesHandler : IRequestHandler<UserSearchQueries, SearchQueryResult<UserSearchItemResponse>>
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<UserSearchQueriesHandler> _logger;
    private readonly IUserSearchQueriesServices _userSearchQueriesServices;
    public UserSearchQueriesHandler(IUserSearchQueriesServices userSearchQueriesServices, ILogger<UserSearchQueriesHandler> logger, ElasticsearchClient client)
    {
        _client = client;
        _logger = logger;
        _userSearchQueriesServices = userSearchQueriesServices;
    }
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

            try
            {
                // Primary path: keep LINQ-style query composition for readability.
                var query = _client.Esql.CreateQuery<UserSearch>().From(ConstantIndexElastic.UserIndex);

                if (!string.IsNullOrWhiteSpace(request.Email))
                    query = query.Where(u => u.Email.Contains(request.Email));

                if (!string.IsNullOrWhiteSpace(request.Username))
                    query = query.Where(u => u.Username.Contains(request.Username));

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    query = query.Where(u => u.PhoneNumber == request.PhoneNumber);

                if (request.IsActive.HasValue)
                    query = query.Where(u => u.IsActive == request.IsActive.Value);

                if (request.StartCreatedAt.HasValue)
                    query = query.Where(u => u.CreatedAt >= request.StartCreatedAt.Value);

                if (request.EndCreatedAt.HasValue)
                    query = query.Where(u => u.CreatedAt <= request.EndCreatedAt.Value);

                if (!string.IsNullOrWhiteSpace(request.SearchAfterCreatedAt) && DateTime.TryParse(request.SearchAfterCreatedAt, out var searchAfterCreatedAt))
                {
                    query = request.OrderByDescending
                        ? query.Where(u => u.CreatedAt < searchAfterCreatedAt)
                        : query.Where(u => u.CreatedAt > searchAfterCreatedAt);
                }

                query = request.SortBy switch
                {
                    UserSortBy.UpdatedAt => request.OrderByDescending
                        ? query.OrderByDescending(u => u.UpdatedAt)
                        : query.OrderBy(u => u.UpdatedAt),
                    UserSortBy.CurrentLevel => request.OrderByDescending
                        ? query.OrderByDescending(u => u.CurrentLevel)
                        : query.OrderBy(u => u.CurrentLevel),
                    _ => request.OrderByDescending
                        ? query.OrderByDescending(u => u.CreatedAt)
                        : query.OrderBy(u => u.CreatedAt)
                };

                query = query.Take(request.Take + 1);

                var documentsFromLinq = await query.ToAsyncEnumerable().ToListAsync(cancellationToken);
                return BuildPagedResult(documentsFromLinq, request.Take);
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                _logger.LogWarning(ex, "ES|QL LINQ query failed. Falling back to Search API query.");
            }

            // Fallback path: if LINQ-style query fails (e.g. due to complex query or deserialization issues), use low-level Search API with manual query construction.
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

                switch (request.SortBy)
                {
                    case UserSortBy.UpdatedAt:
                        s.Sort(sort =>
                        {
                            if (request.OrderByDescending)
                            {
                                sort.Field(f => f.Field(u => u.UpdatedAt).Order(Elastic.Clients.Elasticsearch.SortOrder.Desc));
                            }
                            else
                            {
                                sort.Field(f => f.Field(u => u.UpdatedAt).Order(Elastic.Clients.Elasticsearch.SortOrder.Asc));
                            }
                        });
                        break;
                    case UserSortBy.CurrentLevel:
                        s.Sort(sort =>
                        {
                            if (request.OrderByDescending)
                            {
                                sort.Field(f => f.Field(u => u.CurrentLevel).Order(Elastic.Clients.Elasticsearch.SortOrder.Desc));
                            }
                            else
                            {
                                sort.Field(f => f.Field(u => u.CurrentLevel).Order(Elastic.Clients.Elasticsearch.SortOrder.Asc));
                            }
                        });
                        break;
                    default:
                        s.Sort(sort =>
                        {
                            if (request.OrderByDescending)
                            {
                                sort.Field(f => f.Field(u => u.CreatedAt).Order(Elastic.Clients.Elasticsearch.SortOrder.Desc));
                            }
                            else
                            {
                                sort.Field(f => f.Field(u => u.CreatedAt).Order(Elastic.Clients.Elasticsearch.SortOrder.Asc));
                            }
                        });
                        break;
                }
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new Exception($"Failed to search users: {response.DebugInformation}");
            }

            var documents = response.Documents.ToList();
            return BuildPagedResult(documents, request.Take);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchInternalAsync");
            throw;
        }
    }

    private SearchQueryResult<UserSearch> BuildPagedResult(List<UserSearch> documents, int take)
    {
        _logger.LogInformation("Query executed successfully: {DocumentCount} documents returned", documents.Count);

        var hasNextPage = documents.Count > take;
        var results = documents.Take(take).ToList();

        return new SearchQueryResult<UserSearch>(
            Total: results.Count,
            Items: results,
            HasNextPage: hasNextPage,
            NextCursor: hasNextPage && results.Count > 0 ? results.Last().CreatedAt.ToString("O") : string.Empty);
    }
}
