using HanziAnhVu.Shared.EventBus.Abstracts;
using Microsoft.Extensions.DependencyInjection;
namespace HanziAnhVu.Shared.EventBus.InMemory
{
    // In-memory implementation of the event bus, which directly invokes handlers without any external messaging infrastructure
    public class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
    {
        // Publish an event of type T to all registered handlers
        public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IntegrationEvent
        {
            // Get all handlers for the event type T and invoke them
            var handlers = serviceProvider.GetServices<IIntegrationEventHandler<T>>();
            var tasks = handlers.Select(handler => handler.HandleAsync(@event, ct));
            return Task.WhenAll(tasks); // wait for all handlers to complete
        }


        // Subscribe a handler of type THandler to events of type T
        public void Subcribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            // Với InMemory — Subscribe được handle qua DI registration
            // Không cần làm gì ở đây
        }
    }
}
