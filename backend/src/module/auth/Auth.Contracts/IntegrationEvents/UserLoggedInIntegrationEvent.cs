namespace Auth.Contracts.IntegrationEvents;

public record UserLoggedInIntegrationEvent(string Username, string LoginType) : IntegrationEvent;
