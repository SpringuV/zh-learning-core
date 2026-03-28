namespace Auth.Application.DomainEventsInternal;

public record AuthUserEmailChangedDomainEvent(Guid UserId, string NewEmail, string OldEmail) : BaseDomainEvent, INotification;
