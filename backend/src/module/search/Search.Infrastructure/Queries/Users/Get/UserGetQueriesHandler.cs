namespace Search.Infrastructure.Queries.Users.Get;

public class UserGetQueriesHandler(
    ElasticsearchClient client,
    ILogger<UserGetQueriesHandler> logger) : IRequestHandler<UserGetQueries, UserSearchResponse?>
{
    private readonly ElasticsearchClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<UserGetQueriesHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UserSearchResponse?> Handle(UserGetQueries request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        _logger.LogDebug("Getting user {UserId}", request.Id);

        try
        {
            var response = await _client.GetAsync<UserSearchResponse>(
                request.Id,
                g => g.Index(ConstantIndexElastic.UserIndex),
                cancellationToken);

            if (response.Source != null)
            {
                _logger.LogInformation("Successfully retrieved user {UserId}", request.Id);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found", request.Id);
            }

            return response.Source;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", request.Id);
            throw;
        }
    }

    private static void ValidateRequest(UserGetQueries request)
    {
        if (string.IsNullOrWhiteSpace(request.Id.ToString()))
            throw new ArgumentException("User ID không được để trống.", nameof(request.Id));
    }
}
