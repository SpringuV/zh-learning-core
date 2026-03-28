
// - nơi này giống như dto cho cái event bus
// - khi một user mới được tạo, chúng ta sẽ publish một UserCreatedDomainEvent
// để các handler khác trong cùng module có thể lắng nghe và thực hiện các hành động liên quan
// (ví dụ: gửi email xác nhận, log hoạt động, v.v.)
-----------------------
// - là domain event nội bộ (MediaR.INotification)
-----------------------
// - còn UserRegisteredIntegrationEvent mới là cái gần với DTO cho event bus hơn,
// vì nó được publish ra ngoài qua IEventBus