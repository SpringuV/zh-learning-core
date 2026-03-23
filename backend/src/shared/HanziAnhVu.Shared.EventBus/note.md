HanziAnhVu.Shared.EventBus.csproj
Dùng cho integration event để publish ra ngoài qua event bus.
•	IIntegrationEvent
•	IntegrationEvent
•	IEventBus
•	IIntegrationEventHandler<T>
Ý nghĩa:
•	event này là message để module khác nhận
•	giống payload của message bus hơn
•	ví dụ:
•	UserRegisteredIntegrationEvent
***************************************************************
•	DomainEvent = chuyện nội bộ trong app/module
•	IntegrationEvent = message đem đi giao tiếp giữa module/service