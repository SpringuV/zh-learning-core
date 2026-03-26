namespace Auth.Contracts.IntegrationEvents;

public record UserRegisteredIntegrationEvent(Guid UserId, string Email, string Username, DateTime CreatedAt, string CodeActivate) : IntegrationEvent;
