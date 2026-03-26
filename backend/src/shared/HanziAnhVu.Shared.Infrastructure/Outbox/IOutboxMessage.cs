using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public interface IOutboxMessage
{
    Guid Id { get; set; }
    string Type { get; set; }
    JsonElement Payload { get; set; }
    DateTime OccurredOnUtc { get; set; }
    DateTime? ProcessedOnUtc { get; set; }
    string? Error { get; set; }
    int RetryCount { get; set; }
}
