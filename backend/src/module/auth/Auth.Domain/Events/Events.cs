using HanziAnhVu.Shared.Domain;


namespace Auth.Domain.Events
{
    // Auth.Domain/Events/
    public record UserCreatedDomainEvent(Guid UserId, string Email) : BaseDomainEvent;

    public record UserActivatedDomainEvent(Guid UserId) : BaseDomainEvent;

    public record UserPasswordChangedDomainEvent(Guid UserId) : BaseDomainEvent;

    public record UserEmailChangedDomainEvent(Guid UserId, string NewEmail) : BaseDomainEvent;

    public record UserDeactivatedDomainEvent(Guid UserId) : BaseDomainEvent;
}
