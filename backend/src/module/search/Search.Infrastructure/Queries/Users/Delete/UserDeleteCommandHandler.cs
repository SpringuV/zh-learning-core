namespace Search.Infrastructure.Queries.Users.Delete;
public record UserDeleteCommand(Guid Id) : IRequest<bool>;
public class UserDeleteCommandHandler(
    ElasticsearchClient client,
    ILogger<UserDeleteCommandHandler> logger) : IRequestHandler<UserDeleteCommand, bool>
{
    private readonly ElasticsearchClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<UserDeleteCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> Handle(UserDeleteCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        _logger.LogDebug("Deleting user {UserId}", request.Id);

        try
        {
            var response = await _client.DeleteAsync(
                request.Id,
                d => d.Index(ConstantIndexElastic.UserIndex),
                cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException(
                    $"Failed to delete user {request.Id}: {response.DebugInformation}");
            }

            _logger.LogInformation("Successfully deleted user {UserId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}", request.Id);
            throw;
        }
    }

    private static void ValidateRequest(UserDeleteCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Id.ToString()))
            throw new ArgumentException("UserId không được để trống.", nameof(request.Id));
    }
}
