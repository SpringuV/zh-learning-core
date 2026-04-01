using Elastic.Clients.Elasticsearch;
using Elastic.Esql.Extensions;
using HanziAnhVu.Shared.Application;
using Microsoft.Extensions.Logging;
using Search.Contracts.DTOs;
using Search.Contracts.Interfaces;
using Search.Domain.Entities;

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

public class UserSearchService : IUserSearchProjector, IUserSearchQueries
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<UserSearchService> _logger;

    public UserSearchService(ElasticsearchClient client, ILogger<UserSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexAsync(Guid id, string email, string username, bool isActive, string? phoneNumber, DateTime createdAt, DateTime updatedAt, CancellationToken cancellationToken = default)
    {
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
        if (patch.StreakCount.HasValue) partialDocument["streakCount"] = patch.StreakCount.Value;
        if (patch.CurrentLevel.HasValue) partialDocument["currentLevel"] = patch.CurrentLevel.Value;
        if (patch.TotalExperience.HasValue) partialDocument["totalExperience"] = patch.TotalExperience.Value;
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
                CurrentLevel: user.CurrentLevel,
                TotalExperience: user.TotalExperience,
                StreakCount: user.StreakCount))
            .ToList();

        return new SearchQueryResult<UserSearchItem>(
            Total: searchResult.Total,
            Items: items,
            HasNextPage: searchResult.HasNextPage,
            NextCursor: searchResult.NextCursor);
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
        var query = _client.Esql.CreateQuery<UserSearch>().From(ConstantIndexElastic.UserIndex);
        // fluent chaining
        if (!string.IsNullOrEmpty(Email))
            query = query.Where(u => u.Email.Contains(Email));

        if (!string.IsNullOrEmpty(Username))
            query = query.Where(u => u.Username.Contains(Username));

        if (!string.IsNullOrEmpty(PhoneNumber))
            query = query.Where(u => u.PhoneNumber == PhoneNumber);

        if (IsActive.HasValue)
            query = query.Where(u => u.IsActive == IsActive.Value);

        if (StartCreatedAt.HasValue)
            query = query.Where(u => u.CreatedAt >= StartCreatedAt.Value);

        if (EndCreatedAt.HasValue)
            query = query.Where(u => u.CreatedAt <= EndCreatedAt.Value);

        // key set pagination: nếu SearchAfterCreatedAt có giá trị, sẽ lấy những document có CreatedAt nhỏ hơn timestamp này
        if (!string.IsNullOrEmpty(SearchAfterCreatedAt) && DateTime.TryParse(SearchAfterCreatedAt, out var searchAfterCreatedAt))
        {
            query = OrderByDescending
                ? query.Where(u => u.CreatedAt < searchAfterCreatedAt) // nếu sắp xếp giảm dần, sẽ lấy những document có CreatedAt nhỏ hơn timestamp này
                : query.Where(u => u.CreatedAt > searchAfterCreatedAt); // nếu sắp xếp tăng dần, sẽ lấy những document có CreatedAt lớn hơn timestamp này
        }

        // sắp xếp kết quả theo trường được chỉ định, mặc định là CreatedAt giảm dần (mới nhất trước)
        query = SortBy switch
        {
            UserSortBy.UpdatedAt => OrderByDescending
                ? query.OrderByDescending(u => u.UpdatedAt)
                : query.OrderBy(u => u.UpdatedAt),
            UserSortBy.CurrentLevel => OrderByDescending
                ? query.OrderByDescending(u => u.CurrentLevel)
                : query.OrderBy(u => u.CurrentLevel),
            UserSortBy.TotalExperience => OrderByDescending
                ? query.OrderByDescending(u => u.TotalExperience)
                : query.OrderBy(u => u.TotalExperience),
            UserSortBy.StreakCount => OrderByDescending
                ? query.OrderByDescending(u => u.StreakCount)
                : query.OrderBy(u => u.StreakCount),
            _ => OrderByDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt)
        };
        query = query.Take(Take + 1); // +1 check has next page

        // Log search parameters at debug level
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Searching users: email={Email}, username={Username}, isActive={IsActive}, phoneNumber={PhoneNumber}, take={Take}",
                Email, Username, IsActive, PhoneNumber, Take);
        }

        // execute and convert to list
        var documents = await query.ToAsyncEnumerable().ToListAsync(cancellationToken);
        var hasNextPage = documents.Count > Take;
        var results = documents.Take(Take).ToList();

        // Log search completion
        _logger.LogInformation("User search completed: {ResultCount} documents found, hasNextPage={HasNextPage}",
            results.Count, hasNextPage);

        return new SearchQueryResult<UserSearch>(
            Total: results.Count,
            Items: results,
            HasNextPage: hasNextPage,
            NextCursor: hasNextPage ? results.Last().CreatedAt.ToString("O") : string.Empty);
    }
}