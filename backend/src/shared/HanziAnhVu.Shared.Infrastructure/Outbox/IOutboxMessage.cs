using System.Text.Json;

namespace Shared.Infrastructure.Outbox;

public interface IOutboxMessage
{
    Guid Id { get; set; }
    string Type { get; set; }
    JsonElement Payload { get; set; }
    DateTime OccurredOnUtc { get; set; } // event xảy ra khi nào
    DateTime? ProcessedOnUtc { get; set; } // đã được xử lý lúc nào, null là chưa được xử lý
    string? Error { get; set; }
    int RetryCount { get; set; }
}
