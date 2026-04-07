using System.Reflection;
using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

// This class provides methods for serializing integration events into outbox messages and deserializing them back.
public static class OutboxMessageSerialization
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    // TOutboxMessage is the type of the outbox message entity, which must implement IOutboxMessage and have a parameterless constructor.
    // it is the generic type parameter to allow flexibility in the outbox message entity type used by different applications,
    // while ensuring it adheres to the IOutboxMessage contract.
    // Method Factory: Create an outbox message from an integration event, extracting required properties and serializing the event payload.
    public static TOutboxMessage Create<TOutboxMessage>(object integrationEvent)
        where TOutboxMessage : class, IOutboxMessage, new()
    {
        // get type
        var eventType = integrationEvent.GetType();

        // create new outbox message with required properties extracted from the event and payload serialized as JSON
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

    // Helper method to extract required properties from the integration event using reflection.
    // It ensures that the event has the necessary properties (EventId and OccurredAt) 
    // and that they are of the correct types, throwing exceptions if not.
    private static TProperty GetRequiredProperty<TProperty>(object instance, string propertyName)
    {
        // bind flags to look for public instance properties
        // get the property info using reflection
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
