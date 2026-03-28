namespace Auth.Contracts.IntegrationEvents;

public record UserMailResentIntegrationEvent(string Email, string ActivationLink, string ResendLink, DateTime ExpiredActivation) : IntegrationEvent;
