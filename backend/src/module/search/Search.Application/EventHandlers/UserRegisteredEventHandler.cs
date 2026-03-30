using Auth.Contracts.IntegrationEvents;
using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.Logging;
using Search.Application.Interfaces;
using Search.Domain.Entities;

namespace Search.Application.EventHandlers;

public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IElasticSearchBase<UserSearchDocument> _userSearchService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(IElasticSearchBase<UserSearchDocument> userSearchService, ILogger<UserRegisteredEventHandler> logger)
    {
        _userSearchService = userSearchService;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken ct = default!)
    {
        _logger.LogInformation("Indexing user {UserId} into Elasticsearch", @event.UserId);

        var userDocument = new UserSearchDocument
        {
            Id = @event.UserId.ToString(),
            Email = @event.Email,
            Username = @event.Username,
            CreatedAt = @event.CreatedAt,
            IsActive = false // Default, can be updated later
        };

        await _userSearchService.IndexAsync(userDocument, ct);

        _logger.LogInformation("User {UserId} indexed successfully", @event.UserId);
    }
}