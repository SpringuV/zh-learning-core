using Auth.Contracts;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Logging;
using Search.Contracts.DTOs;
using Search.Application.Interfaces;

namespace Search.Application.EventHandlers.Users;

public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IUserSearchQueriesServices _userSearchServices;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(IUserSearchQueriesServices userSearchServices, ILogger<UserRegisteredEventHandler> logger)
    {
        _userSearchServices = userSearchServices;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default!)
    {
        // Log search parameters at debug level
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Indexing user {UserId} into Elasticsearch", @event.UserId);
        }

        var request = new UserSearchIndexQueriesRequest(
            Id: @event.UserId,
            Email: @event.Email,
            Username: @event.Username,
            IsActive: false,
            PhoneNumber: null,
            CreatedAt: @event.CreatedAt,
            UpdatedAt: @event.CreatedAt);

        await _userSearchServices.IndexAsync(request, ct);

        _logger.LogInformation("User {UserId} indexed successfully", @event.UserId);
    }
}