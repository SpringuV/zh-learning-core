using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public abstract class OutboxMessageBase : IOutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public JsonElement Payload { get; set; }
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
