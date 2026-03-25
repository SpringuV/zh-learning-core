using System.Text.Json;

namespace Auth.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    // event type, ex: UserLoggedInIntegrationEvent
    public string Type { get; set; } = null!;
    // json
    public JsonElement Payload { get; set; }
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedOnUtc { get; set; } = default!;

    public string? Error { get; set; } = default!;

    public int RetryCount { get; set; } = 0;
}
