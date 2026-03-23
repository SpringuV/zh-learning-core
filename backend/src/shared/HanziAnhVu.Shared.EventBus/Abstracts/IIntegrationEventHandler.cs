namespace HanziAnhVu.Shared.EventBus.Abstracts
{
    public interface IIntegrationEventHandler<in T> where T : IIntegrationEvent
    {
        Task HandleAsync(T @event, CancellationToken ct = default);
    }
}
