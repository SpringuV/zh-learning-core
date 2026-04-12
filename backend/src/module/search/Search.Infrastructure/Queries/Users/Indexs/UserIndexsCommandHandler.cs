namespace Search.Infrastructure.Queries.Users.Indexs;

public record UserIndexsCommand(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt) : IRequest<UserIndexResponse>;

public class UserCreateIndexCommandHandler(
    ElasticsearchClient client,
    ILogger<UserCreateIndexCommandHandler> logger) : IRequestHandler<UserIndexsCommand, UserIndexResponse>
{
    private readonly ElasticsearchClient _client = client;
    private readonly ILogger<UserCreateIndexCommandHandler> _logger = logger;

    public async Task<UserIndexResponse> Handle(UserIndexsCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        _logger.LogDebug("Indexing user {UserId} with email {Email}", request.Id, request.Email);

        try
        {
            // Ensure index exists
            await EnsureUserIndexExistsAsync(cancellationToken);

            // Create UserSearch entity
            var userSearch = new UserSearch(
                Id: request.Id,
                Email: request.Email,
                Username: request.Username,
                PhoneNumber: request.PhoneNumber,
                CreatedAt: request.CreatedAt,
                UpdatedAt: request.UpdatedAt,
                IsActive: request.IsActive);

            // Index to Elasticsearch
            var response = await _client.IndexAsync(userSearch, i => i
                .Index(ConstantIndexElastic.UserIndex)
                .Id(userSearch.Id), cancellationToken); // Use userSearch.Id to ensure the document ID in Elasticsearch matches the user ID

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException(
                    $"Failed to index user {request.Id}: {response.DebugInformation}");
            }

            _logger.LogInformation("Successfully indexed user {UserId}", request.Id);

            return new UserIndexResponse(
                UserId: request.Id,
                IndexedAt: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index user {UserId}", request.Id);
            throw;
        }
    }

    private static void ValidateRequest(UserIndexsCommand request)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(request.Id));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email cannot be empty", nameof(request.Email));

        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username cannot be empty", nameof(request.Username));

        if (request.CreatedAt == default)
            throw new ArgumentException("CreatedAt cannot be empty", nameof(request.CreatedAt));

        if (request.UpdatedAt == default)
            throw new ArgumentException("UpdatedAt cannot be empty", nameof(request.UpdatedAt));
    }

    private async Task EnsureUserIndexExistsAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ConstantIndexElastic.UserIndex, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        // Create explicit mapping to keep search/sort behavior stable across environments.
        var createResponse = await _client.Indices.CreateAsync(ConstantIndexElastic.UserIndex, c => c
            .Mappings<UserSearch>(m => m
                .Properties(p => p
                    .Text(u => u.Id, t => t.Fields(f => f.Keyword("keyword")))
                    .Text(u => u.Email, t => t.Fields(f => f.Keyword("keyword")))
                    .Text(u => u.Username, t => t.Fields(f => f.Keyword("keyword")))
                    .Text(u => u.PhoneNumber, t => t.Fields(f => f.Keyword("keyword")))
                    .Boolean(u => u.IsActive)
                    .Date(u => u.CreatedAt)
                    .Date(u => u.UpdatedAt)
                    .IntegerNumber(u => u.CurrentLevel)
                    .Date(u => u.LastLogin)
                    .Keyword(u => u.AvatarUrl)
                )
            ), cancellationToken: cancellationToken);
        
        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Failed to create index {ConstantIndexElastic.UserIndex}: {createResponse.DebugInformation}");
        }

        _logger.LogInformation("Created missing index {IndexName}", ConstantIndexElastic.UserIndex);
    }
}
