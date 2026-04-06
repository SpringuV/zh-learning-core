using HanziAnhVu.Shared.EventBus;

namespace HanziAnhVu.Shared.Application;

public interface IOutboxWriter
{
    Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
