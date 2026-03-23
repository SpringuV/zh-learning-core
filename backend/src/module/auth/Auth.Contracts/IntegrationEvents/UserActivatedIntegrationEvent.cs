namespace Auth.Contracts.IntegrationEvents;

public record UserActivatedIntegrationEvent(Guid UserId) : IntegrationEvent;
