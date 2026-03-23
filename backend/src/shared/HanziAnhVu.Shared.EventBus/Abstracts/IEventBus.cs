namespace HanziAnhVu.Shared.EventBus.Abstracts
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent;

        // register handler for event
        // T: event type
        // THandler: handler type
        void Subcribe<T, THandler>() 
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>;
    }
}
