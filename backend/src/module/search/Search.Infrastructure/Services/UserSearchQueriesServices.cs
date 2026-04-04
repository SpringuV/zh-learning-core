using Elastic.Clients.Elasticsearch;
using Elastic.Esql.Extensions;
using HanziAnhVu.Shared.Application;
using Microsoft.Extensions.Logging;
using Search.Contracts.DTOs;
using Search.Contracts.Interfaces;
using Search.Domain.Entities;
using System.Text.Json;

namespace Search.Infrastructure.Services;

/*
    Có thể tách nhiều index theo mục đích query:
        hanzi_users (search user tổng quát)
        hanzi_users_admin (nhiều field cho admin filter/sort)
        hanzi_users_analytics (dữ liệu tối ưu cho dashboard thống kê)
    Cũng có thể versioning:
        hanzi_users_v1
        hanzi_users_v2
        rồi dùng alias hanzi_users trỏ sang version active để reindex zero-downtime.
 */

public class UserSearchQueriesServices : IUserSearchProjector, IUserSearchQueriesServices
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<UserSearchQueriesServices> _logger;

    public UserSearchQueriesServices(ElasticsearchClient client, ILogger<UserSearchQueriesServices> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexAsync(Guid id, string email, string username, bool isActive, string? phoneNumber, DateTime createdAt, DateTime updatedAt, CancellationToken cancellationToken = default)
    {
        await EnsureUserIndexExistsAsync(cancellationToken);

        var userSearch = UserSearch.FromUser(new UserSearchDoumentDTOs(
            Id: id,
            Email: email,
            Username: username,
            CreatedAt: createdAt,
            UpdatedAt: updatedAt,
            IsActive: isActive,
            PhoneNumber: phoneNumber));

        var response = await _client.IndexAsync(userSearch, i => i
            .Index(ConstantIndexElastic.UserIndex)
            .Id(userSearch.Id), cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to index user: {response.DebugInformation}");
        }
    }

    public async Task PatchAsync(string id, UserSearchPatchDocument patch, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Document id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(patch);

        var partialDocument = new Dictionary<string, object>();
        if (patch.Email is not null) partialDocument["email"] = patch.Email;
        if (patch.Username is not null) partialDocument["username"] = patch.Username;
        if (patch.PhoneNumber is not null) partialDocument["phoneNumber"] = patch.PhoneNumber;
        if (patch.IsActive.HasValue) partialDocument["isActive"] = patch.IsActive.Value;
        if (patch.LastLogin.HasValue) partialDocument["lastLogin"] = patch.LastLogin.Value;
        if (patch.CurrentLevel.HasValue) partialDocument["currentLevel"] = patch.CurrentLevel.Value;
        if (patch.LastActivityAt.HasValue) partialDocument["lastActivityAt"] = patch.LastActivityAt.Value;
        if (patch.AvatarUrl is not null) partialDocument["avatarUrl"] = patch.AvatarUrl;
        if (patch.UpdatedAt.HasValue) partialDocument["updatedAt"] = patch.UpdatedAt.Value;

        if (partialDocument.Count == 0)
        {
            throw new ArgumentException("At least one field must be provided for patch.", nameof(patch));
        }

        var response = await _client.UpdateAsync<UserSearch, Dictionary<string, object>>(ConstantIndexElastic.UserIndex, id, u => u
            .Doc(partialDocument)
            .RetryOnConflict(3), cancellationToken); // dữ liệu trả về là document sau khi update, nếu muốn lấy document 
                                                    // trước khi update thì dùng .Source(false)

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to patch user {id}: {response.DebugInformation}");
        }
    }

    public async Task<UserSearch?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<UserSearch>(id, g => g.Index(ConstantIndexElastic.UserIndex), cancellationToken); // 1
        return response.Source; // 2
        // 1: The GetResponse is mapped 1-to-1 with the Elasticsearch JSON response.
        // 2: The original document is deserialized as an instance of the UserSearch class, accessible on the response via the Source property.
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync(id, d => d.Index(ConstantIndexElastic.UserIndex), cancellationToken);
    }

    public async Task<SearchQueryResult<UserSearchItem>> SearchUsersAsync(UserSearchQueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchResult = await SearchInternalAsync(
                Email: request.Email,
                Username: request.Username,
                IsActive: request.IsActive,
                PhoneNumber: request.PhoneNumber,
                Take: request.Take,
                SearchAfterCreatedAt: request.SearchAfterCreatedAt,
                SortBy: request.SortBy,
                OrderByDescending: request.OrderByDescending,
                StartCreatedAt: request.StartCreatedAt,
                EndCreatedAt: request.EndCreatedAt,
                cancellationToken: cancellationToken);

            var items = searchResult.Items
                .Select(user => new UserSearchItem(
                    Id: user.Id,
                    Email: user.Email,
                    Username: user.Username,
                    PhoneNumber: user.PhoneNumber,
                    IsActive: user.IsActive,
                    CreatedAt: user.CreatedAt,
                    UpdatedAt: user.UpdatedAt,
                    CurrentLevel: user.CurrentLevel))
                .ToList();

            return new SearchQueryResult<UserSearchItem>(
                Total: searchResult.Total,
                Items: items,
                HasNextPage: searchResult.HasNextPage,
                NextCursor: searchResult.NextCursor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with request: {@Request}", request);
            throw;
        }
    }

    private async Task<SearchQueryResult<UserSearch>> SearchInternalAsync(
        string? Email = null,
        string? Username = null,
        bool? IsActive = null,
        string? PhoneNumber = null,
        int Take = 30,
        string? SearchAfterCreatedAt = null,
        UserSortBy SortBy = UserSortBy.CreatedAt,
        bool OrderByDescending = true,
        DateTime? StartCreatedAt = null,
        DateTime? EndCreatedAt = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchRequest = new Search.Contracts.DTOs.UserSearchQueryRequest(
                Email: Email,
                Username: Username,
                IsActive: IsActive,
                PhoneNumber: PhoneNumber,
                Take: Take + 1,
                SearchAfterCreatedAt: SearchAfterCreatedAt,
                SortBy: SortBy,
                OrderByDescending: OrderByDescending,
                StartCreatedAt: StartCreatedAt,
                EndCreatedAt: EndCreatedAt);

            _logger.LogDebug("Executing search with request: {@SearchRequest}", searchRequest);

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

            // Primary path: keep LINQ-style query composition for readability.
            var query = _client.Esql.CreateQuery<UserSearch>().From(ConstantIndexElastic.UserIndex);

            if (!string.IsNullOrWhiteSpace(Email))
                query = query.Where(u => u.Email.Contains(Email));

            if (!string.IsNullOrWhiteSpace(Username))
                query = query.Where(u => u.Username.Contains(Username));

            if (!string.IsNullOrWhiteSpace(PhoneNumber))
                query = query.Where(u => u.PhoneNumber == PhoneNumber);

            if (IsActive.HasValue)
                query = query.Where(u => u.IsActive == IsActive.Value);

            if (StartCreatedAt.HasValue)
                query = query.Where(u => u.CreatedAt >= StartCreatedAt.Value);

            if (EndCreatedAt.HasValue)
                query = query.Where(u => u.CreatedAt <= EndCreatedAt.Value);

            if (!string.IsNullOrWhiteSpace(SearchAfterCreatedAt) && DateTime.TryParse(SearchAfterCreatedAt, out var searchAfterCreatedAt))
            {
                query = OrderByDescending
                    ? query.Where(u => u.CreatedAt < searchAfterCreatedAt)
                    : query.Where(u => u.CreatedAt > searchAfterCreatedAt);
            }

            query = SortBy switch
            {
                UserSortBy.UpdatedAt => OrderByDescending
                    ? query.OrderByDescending(u => u.UpdatedAt)
                    : query.OrderBy(u => u.UpdatedAt),
                UserSortBy.CurrentLevel => OrderByDescending
                    ? query.OrderByDescending(u => u.CurrentLevel)
                    : query.OrderBy(u => u.CurrentLevel),
                _ => OrderByDescending
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt)
            };

            query = query.Take(Take + 1);

            try
            {
                var documentsFromLinq = await query.ToAsyncEnumerable().ToListAsync(cancellationToken);
                return BuildPagedResult(documentsFromLinq, Take);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "ES|QL LINQ materialization failed. Falling back to Search API query.");
            }

            // Fallback path: if LINQ-style query fails (e.g. due to complex query or deserialization issues), use low-level Search API with manual query construction.
            var response = await _client.SearchAsync<UserSearch>(s =>
            {
                s.Indices(ConstantIndexElastic.UserIndex);
                s.Size(Take + 1);

                s.Query(q => q.Bool(b =>
                {
                    if (!string.IsNullOrWhiteSpace(Email))
                    {
                        b.Filter(f => f.Match(m => m.Field(u => u.Email).Query(Email)));
                    }

                    if (!string.IsNullOrWhiteSpace(Username))
                    {
                        b.Filter(f => f.Match(m => m.Field(u => u.Username).Query(Username)));
                    }

                    if (!string.IsNullOrWhiteSpace(PhoneNumber))
                    {
                        b.Filter(f => f.MatchPhrase(m => m.Field(u => u.PhoneNumber).Query(PhoneNumber)));
                    }

                    if (IsActive.HasValue)
                    {
                        b.Filter(f => f.Term(t => t.Field(u => u.IsActive).Value(IsActive.Value)));
                    }

                    if (StartCreatedAt.HasValue || EndCreatedAt.HasValue)
                    {
                        b.Filter(f => f.Range(r => r.Date(dr =>
                        {
                            dr.Field(u => u.CreatedAt);
                            if (StartCreatedAt.HasValue)
                            {
                                dr.Gte(StartCreatedAt.Value);
                            }

                            if (EndCreatedAt.HasValue)
                            {
                                dr.Lte(EndCreatedAt.Value);
                            }

                        })));
                    }

                    if (!string.IsNullOrWhiteSpace(SearchAfterCreatedAt) && DateTime.TryParse(SearchAfterCreatedAt, out var searchAfterCreatedAt))
                    {
                        b.Filter(f => f.Range(r => r.Date(dr =>
                        {
                            dr.Field(u => u.CreatedAt);
                            if (OrderByDescending)
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

                switch (SortBy)
                {
                    case UserSortBy.UpdatedAt:
                        s.Sort(sort =>
                        {
                            if (OrderByDescending)
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
                            if (OrderByDescending)
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
                            if (OrderByDescending)
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
            return BuildPagedResult(documents, Take);
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

    private async Task EnsureUserIndexExistsAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.UserIndex, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        var createResponse = await _client.Indices.CreateAsync(ConstantIndexElastic.UserIndex, cancellationToken: cancellationToken);
        if (!createResponse.IsValidResponse)
        {
            throw new Exception($"Failed to create index {ConstantIndexElastic.UserIndex}: {createResponse.DebugInformation}");
        }

        _logger.LogInformation("Created missing index {IndexName}", ConstantIndexElastic.UserIndex);
    }
}