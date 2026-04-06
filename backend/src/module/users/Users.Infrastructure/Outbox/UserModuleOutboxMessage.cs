using Shared.Infrastructure.Outbox;

namespace Users.Infrastructure.Outbox;

public sealed class UserModuleOutboxMessage : OutboxMessageBase
{
    // Concrete implementation của OutboxMessageBase cho Users module
    // Giúp phân biệt với AuthOutboxMessage, dễ dàng quản lý từng module riêng
}
