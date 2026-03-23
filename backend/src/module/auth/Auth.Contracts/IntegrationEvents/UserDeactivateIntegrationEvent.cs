namespace Auth.Contracts.IntegrationEvents;

public record UserDeactivateIntegrationEvent(Guid UserId) : IntegrationEvent;
