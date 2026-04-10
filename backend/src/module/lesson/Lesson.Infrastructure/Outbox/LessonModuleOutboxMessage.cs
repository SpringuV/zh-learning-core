namespace Lesson.Infrastructure.Outbox;

public sealed class LessonModuleOutboxMessage : OutboxMessageBase
{
    // Concrete implementation của OutboxMessageBase cho Lesson module
    // Giúp phân biệt với các module khác, dễ dàng quản lý từng module riêng
}