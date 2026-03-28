namespace Auth.Application.DomainEventsInternal;

public record AuthUserMailResentEvent(string Email, string ActivationCode, DateTime ExpiredActivation, string ActivationLink, string ResendLink): BaseDomainEvent, INotification;
