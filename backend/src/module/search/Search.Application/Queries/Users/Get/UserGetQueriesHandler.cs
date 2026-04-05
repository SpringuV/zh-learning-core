using Elastic.Clients.Elasticsearch;
using MediatR;
using Microsoft.Extensions.Logging;
using Search.Domain.Entities;

namespace Search.Application.Queries.Users.Get;

public class UserGetQueriesHandler(
    ElasticsearchClient client,
    ILogger<UserGetQueriesHandler> logger) : IRequestHandler<UserGetQueries, UserSearch?>
{
    private readonly ElasticsearchClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<UserGetQueriesHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UserSearch?> Handle(UserGetQueries request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        _logger.LogDebug("Getting user {UserId}", request.Id);

        try
        {
            var response = await _client.GetAsync<UserSearch>(
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
        if (string.IsNullOrWhiteSpace(request.Id))
            throw new ArgumentException("User ID cannot be empty", nameof(request.Id));
    }
}
