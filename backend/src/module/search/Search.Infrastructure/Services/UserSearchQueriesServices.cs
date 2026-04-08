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
public class UserSearchQueriesServices(ILogger<UserSearchQueriesServices> logger, IMediator mediator) : IUserSearchQueriesServices
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<UserSearchQueriesServices> _logger = logger;

    public async Task<UserIndexResponse> IndexAsync(UserSearchIndexQueriesRequest request, CancellationToken cancellationToken = default)
    {
        var command = new UserIndexsQueries(
            Id: request.Id,
            Email: request.Email,
            Username: request.Username,
            PhoneNumber: request.PhoneNumber,
            CreatedAt: request.CreatedAt,
            UpdatedAt: request.UpdatedAt,
            IsActive: request.IsActive);
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    public async Task<UserSearchPatchDocumentResponse?> PatchAsync(Guid id, UserSearchPatchDocumentRequest patch, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id.ToString()))
        {
            throw new ArgumentException("UserId không được để trống.", nameof(id));
        }
        ArgumentNullException.ThrowIfNull(patch); // Nếu patch có thể null thì bỏ dòng này và handle null trong code tiếp theo
        var command = new UserPatchQueries(
             Id: id,
             Email: patch?.Email,
             Username: patch?.Username,
             IsActive: patch?.IsActive,
             PhoneNumber: patch?.PhoneNumber,
             LastLogin: patch?.LastLogin,
             CurrentLevel: patch?.CurrentLevel,
             LastActivityAt: patch?.LastActivityAt,
             AvatarUrl: patch?.AvatarUrl);

        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    public async Task<UserSearchResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id.ToString()))
            throw new ArgumentException("UserId không được để trống.", nameof(id));

        _logger.LogDebug("Getting user {UserId}", id);

        var query = new UserGetQueries(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id.ToString()))
            throw new ArgumentException("UserId không được để trống.", nameof(id));

        _logger.LogDebug("Deleting user {UserId}", id);

        var command = new UserDeleteCommand(id);
        await _mediator.Send(command, cancellationToken);
    }

    public async Task<SearchQueryResult<UserSearchItemResponse>> SearchUsersAsync(UserSearchQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new UserSearchQueries(
            request.Email,
            request.Username,
            request.IsActive,
            request.PhoneNumber,
            request.Take,
            request.SearchAfterCreatedAt,
            request.SortBy,
            request.OrderByDescending,
            request.StartCreatedAt,
            request.EndCreatedAt);

        _logger.LogDebug("Searching users with request: {@Request}", request);

        var result = await _mediator.Send(command, cancellationToken) 
            ?? new SearchQueryResult<UserSearchItemResponse>(
                Total: 0,
                Items: [],
                HasNextPage: false,
                NextCursor: string.Empty);

        return result;
    }

}