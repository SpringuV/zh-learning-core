using HanziAnhVu.Shared.EventBus;

namespace Auth.Application.Interfaces;

public interface IOutboxWriter
{
    Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
