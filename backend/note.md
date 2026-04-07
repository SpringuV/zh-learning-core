về sau cái module search sẽ subscribe event user registered để tự động index user vào Elasticsearch, phục vụ cho việc search user sau này.

trong RegisterUserCommandHandler bạn chỉ đẩy integration event vào outbox.
Chỗ lắng nghe sẽ là consumer của IEventBus, cụ thể:
• AuthOutboxDispatcherService đọc outbox và gọi IEventBus.PublishAsync(...).
• Một handler kiểu IIntegrationEventHandler<UserRegisteredIntegrationEvent> (ở module search/notification) sẽ subscribe event này.
• Trong handler đó, inject ElasticsearchClient và thực hiện IndexAsync/UpdateAsync để cập nhật index.
Hiện tại code của bạn chưa có handler consumer đó, nên event mới chỉ được publish, chưa có chỗ cập nhật Elasticsearch tự động.
Nếu bạn muốn, tôi có thể tạo luôn:

1. handler UserRegisteredIntegrationEventHandler,
2. đăng ký DI/subscription,
3. logic index vào hanzi_documents (hoặc index riêng cho user).
