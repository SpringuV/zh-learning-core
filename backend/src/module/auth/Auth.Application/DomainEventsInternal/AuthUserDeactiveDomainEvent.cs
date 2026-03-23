namespace Auth.Application.DomainEventsInternal;

public record UserDeactivatedDomainEvent(Guid UserId) : BaseDomainEvent, INotification;