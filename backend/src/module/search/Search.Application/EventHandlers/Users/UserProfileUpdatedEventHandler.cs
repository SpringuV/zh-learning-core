using Auth.Contracts;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Logging;
using Search.Contracts.DTOs;
using Search.Contracts.Interfaces;

namespace Search.Application.EventHandlers.Users;

public class UserProfileUpdatedEventHandler(IUserSearchQueriesServices userSearchServices, ILogger<UserProfileUpdatedEventHandler> logger) : IIntegrationEventHandler<UserProfileUpdatedIntegrationEvent>
{
    private readonly IUserSearchQueriesServices _userSearchServices = userSearchServices;
    private readonly ILogger<UserProfileUpdatedEventHandler> _logger = logger;

    public async Task HandleAsync(UserProfileUpdatedIntegrationEvent @event, CancellationToken ct = default!)
    {
        // Log search parameters at debug level
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Updating user {UserId} profile in Elasticsearch", @event.UserId);
        }
        var request = new UserSearchPatchDocumentRequest(

            PhoneNumber: @event.PhoneNumber,
            AvatarUrl: @event.AvatarUrl,
            UpdatedAt: @event.UpdatedAt); 
        await _userSearchServices.PatchAsync(@event.UserId, request, ct);

        _logger.LogInformation("User {UserId} profile updated successfully in Elasticsearch", @event.UserId);
    }
}