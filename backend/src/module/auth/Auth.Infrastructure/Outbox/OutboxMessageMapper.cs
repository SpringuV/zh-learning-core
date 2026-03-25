using HanziAnhVu.Shared.EventBus;
using System.Text.Json;

namespace Auth.Infrastructure.Outbox;

internal static class OutboxMessageMapper
{
    // Sử dụng JsonSerializerOptions chung để đảm bảo nhất quán trong việc serialize và deserialize,
    // đặc biệt là với các event có generic type parameters
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web) // JsonSerializerDefaults.Web sẽ tự động cấu hình để serialize property names
                                                                                                      // theo camelCase, phù hợp với convention của JSON và giúp tránh lỗi khi deserialize
                                                                                                      // nếu có sự khác biệt về case giữa C# properties và JSON keys
    {
        PropertyNameCaseInsensitive = true
    };

    public static OutboxMessage Create(IntegrationEvent integrationEvent)
    {
        return new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = integrationEvent.GetType().AssemblyQualifiedName ?? integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name,
            // serializer cho tất cả các loại event, bao gồm cả những event có generic type parameters
            Payload = JsonSerializer.SerializeToElement(integrationEvent, integrationEvent.GetType(), SerializerOptions),
            OccurredOnUtc = integrationEvent.OccurredAt
        };
    }

    public static object? DeserializePayload(OutboxMessage message, Type eventType)
    {
        return JsonSerializer.Deserialize(message.Payload.GetRawText(), eventType, SerializerOptions);
    }
}
