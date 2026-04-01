using Auth.Contracts.IntegrationEvents;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Logging;
using Search.Contracts.Interfaces;

namespace Search.Application.EventHandlers.Users;

public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IUserSearchProjector _userSearchProjector;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(IUserSearchProjector userSearchProjector, ILogger<UserRegisteredEventHandler> logger)
    {
        _userSearchProjector = userSearchProjector;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default!)
    {
        // Log search parameters at debug level
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Indexing user {UserId} into Elasticsearch", @event.UserId);
        }

        await _userSearchProjector.IndexAsync(
            id: @event.UserId,
            email: @event.Email,
            username: @event.Username,
            isActive: false,
            phoneNumber: null,
            createdAt: @event.CreatedAt,
            updatedAt: @event.CreatedAt,
            cancellationToken: ct);

        _logger.LogInformation("User {UserId} indexed successfully", @event.UserId);
    }
}