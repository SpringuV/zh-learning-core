namespace Auth.Application.DomainEvents;


// sealed record để đảm bảo tính bất biến và hiệu quả, vì domain event thường chỉ chứa dữ liệu và không có logic phức tạp
public sealed record AuthUserCreatedDomainEvent(Guid UserId, string Email, string UserName, DateTime CreatedAt, string ActivateCode, string ActivationLink, string ResendLink) : BaseDomainEvent, INotification;
