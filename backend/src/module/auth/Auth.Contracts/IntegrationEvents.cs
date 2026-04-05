namespace Auth.Contracts;
/*Integration event này sẽ là nơi chúng ta định nghĩa các sự kiện mà sẽ được publish ra ngoài qua event bus 
(ví dụ: RabbitMQ, Kafka, v.v.) để các hệ thống khác có thể lắng nghe và phản hồi.

- hiện tại trong project đang dùng event bus là inmemory event bus,
- nhưng sau này có thể sẽ chuyển sang một giải pháp khác như RabbitMQ hoặc Kafka, 
- nên cần có một lớp abstraction để dễ dàng thay đổi mà không ảnh hưởng đến phần còn lại của codebase.*/
public record UserActivatedIntegrationEvent(Guid UserId) : IntegrationEvent;
public record UserDeactivateIntegrationEvent(Guid UserId) : IntegrationEvent;
public record UserEmailChangedIntegrationEvent(Guid UserId, string NewEmail) : IntegrationEvent;
public record UserLoggedInIntegrationEvent(string Username, string LoginType) : IntegrationEvent;
public record UserMailResentIntegrationEvent(string Email, string ActivationLink, string ResendLink, DateTime ExpiredActivation) : IntegrationEvent;
public record UserPasswordChangedIntegrationEvent(Guid UserId): IntegrationEvent;
public record UserProfileUpdatedIntegrationEvent(Guid UserId, string PhoneNumber, string AvatarUrl, DateTime UpdatedAt) : IntegrationEvent;
public record UserRegisteredIntegrationEvent(Guid UserId, string Email, string Username, DateTime CreatedAt, string ActivationCode, string ActivationLink, string ResendLink) : IntegrationEvent;
