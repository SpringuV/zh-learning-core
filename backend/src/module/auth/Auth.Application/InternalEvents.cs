namespace Auth.Application;
/*
// - nơi này giống như dto cho cái event bus
// - khi một user mới được tạo, chúng ta sẽ publish một UserCreatedDomainEvent
// để các handler khác trong cùng module có thể lắng nghe và thực hiện các hành động liên quan
// (ví dụ: gửi email xác nhận, log hoạt động, v.v.)
-----------------------
// - là domain event nội bộ (MediaR.INotification)
-----------------------
// - còn UserRegisteredIntegrationEvent mới là cái gần với DTO cho event bus hơn,
// vì nó được publish ra ngoài qua IEventBus
*/
public record AuthUserActivatedDomainEvent(Guid UserId) : BaseDomainEvent, INotification;
// sealed record để đảm bảo tính bất biến và hiệu quả, vì domain event thường chỉ chứa dữ liệu và không có logic phức tạp
public sealed record AuthUserCreatedDomainEvent(Guid UserId, string Email, string UserName, DateTime CreatedAt, string ActivateCode, string ActivationLink, string ResendLink) : BaseDomainEvent, INotification;
public record UserDeactivatedDomainEvent(Guid UserId) : BaseDomainEvent, INotification;
public record AuthUserEmailChangedDomainEvent(Guid UserId, string NewEmail, string OldEmail) : BaseDomainEvent, INotification;
public record AuthUserMailResentEvent(string Email, string ActivationCode, DateTime ExpiredActivation, string ActivationLink, string ResendLink): BaseDomainEvent, INotification;
public record AuthUserProfileUpdatedEvent(Guid UserId, string NewPhoneNumber, string NewAvatarUrl, DateTime UpdatedAt) : BaseDomainEvent, INotification;