// - class này sẽ luôn bắt sự kiện UserCreatedDomainEvent, và khi bắt được sự kiện này,
// nó sẽ publish một UserRegisteredIntegrationEvent ra ngoài qua IEventBus
// - việc này cho phép các module khác (ví dụ: Notification, Logging, v.v.)
// có thể lắng nghe UserRegisteredIntegrationEvent và thực hiện các hành động liên quan