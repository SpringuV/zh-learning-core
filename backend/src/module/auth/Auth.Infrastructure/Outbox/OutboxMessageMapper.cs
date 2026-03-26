using HanziAnhVu.Shared.EventBus;
using Shared.Infrastructure.Outbox;

namespace Auth.Infrastructure.Outbox;

internal static class OutboxMessageMapper
{
    public static OutboxMessage Create(IntegrationEvent integrationEvent)
    {
        return OutboxMessageSerialization.Create<OutboxMessage>(integrationEvent);
    }

    public static object? DeserializePayload(OutboxMessage message, Type eventType)
    {
        return OutboxMessageSerialization.DeserializePayload(message, eventType);
    }
}
