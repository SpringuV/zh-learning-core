using System.Reflection;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public static class OutboxMessageSerialization
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    // TOutboxMessage is the type of the outbox message entity, which must implement IOutboxMessage and have a parameterless constructor.
    // it is the generic type parameter to allow flexibility in the outbox message entity type used by different applications,
    // while ensuring it adheres to the IOutboxMessage contract.
    public static TOutboxMessage Create<TOutboxMessage>(object integrationEvent)
        where TOutboxMessage : class, IOutboxMessage, new()
    {
        // get type
        var eventType = integrationEvent.GetType();

        // create new 
        return new TOutboxMessage
        {
            Id = GetRequiredProperty<Guid>(integrationEvent, "EventId"),
            Type = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name,
            Payload = JsonSerializer.SerializeToElement(integrationEvent, eventType, SerializerOptions),
            OccurredOnUtc = GetRequiredProperty<DateTime>(integrationEvent, "OccurredAt")
        };
    }

    public static object? DeserializePayload(IOutboxMessage message, Type eventType)
    {
        return JsonSerializer.Deserialize(message.Payload.GetRawText(), eventType, SerializerOptions);
    }

    private static TProperty GetRequiredProperty<TProperty>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Event type '{instance.GetType().FullName}' does not expose required property '{propertyName}'.");

        var value = property.GetValue(instance);
        if (value is TProperty typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Property '{propertyName}' on type '{instance.GetType().FullName}' is not '{typeof(TProperty).Name}'.");
    }
}
