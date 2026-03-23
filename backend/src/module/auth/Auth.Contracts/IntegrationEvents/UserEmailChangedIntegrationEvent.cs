namespace Auth.Contracts.IntegrationEvents;

public record UserEmailChangedIntegrationEvent(Guid UserId, string NewEmail) : IntegrationEvent;
