namespace HanziAnhVu.Shared.Domain
{
    public abstract record BaseDomainEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public Guid EventId { get; } = Guid.CreateVersion7();
    }
}
