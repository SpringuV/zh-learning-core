namespace HanziAnhVu.Shared.EventBus.Abstracts
{
    public interface IIntegrationEvent
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
    }
}
