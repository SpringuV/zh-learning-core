namespace HanziAnhVu.Shared.EventBus.Abstracts
{
    // event bus
    // interface này có tác dụng là một contract để các module có thể publish và subscribe
    // events mà không cần biết về cách thức triển khai cụ thể của event bus
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
