using HanziAnhVu.Shared.EventBus.Abstracts;
using System.Text.Json.Serialization;

namespace HanziAnhVu.Shared.EventBus;
// Marker interface — IIntegrationEvent
public abstract record IntegrationEvent : IIntegrationEvent
{
    // each event has unique Id and creation time for better tracking and debugging
    public IntegrationEvent()
    {
        Id = Guid.CreateVersion7();
        CreationDate = DateTime.UtcNow;
    }

    [JsonInclude]
    public Guid Id { get; set; }

    [JsonInclude]
    public DateTime CreationDate { get; set; }

    public Guid EventId => Id;

    public DateTime OccurredAt => CreationDate;
}
