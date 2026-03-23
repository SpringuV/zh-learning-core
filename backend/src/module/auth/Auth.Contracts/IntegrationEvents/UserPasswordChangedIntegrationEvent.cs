namespace Auth.Contracts.IntegrationEvents;

public record UserPasswordChangedIntegrationEvent(Guid userId): IntegrationEvent;
