namespace HanziAnhVu.Shared.Domain
{
    public interface IDomainEvent
    {
        // Để debug, log biết event xảy ra lúc nào
        DateTime OccurredAt { get; }

        // Để idempotency — tránh xử lý 2 lần cùng 1 event
        Guid EventId { get; }
        /*
        CREATE TABLE DomainEvents (
            Id UUID PRIMARY KEY,
            AggregateId UUID NOT NULL,           -- Course, Topic, Exercise, User... id
            AggregateType VARCHAR(100) NOT NULL,  -- "Course", "Topic", "Exercise", "User"
            EventType VARCHAR(200) NOT NULL,      -- "CourseCreatedEvent", "TopicPublishedEvent"
            Payload JSONB NOT NULL,               -- Event data
            Version INT NOT NULL,                 -- Event version for aggregate
            OccurredAt TIMESTAMP NOT NULL,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            
            CONSTRAINT unique_aggregate_version UNIQUE(AggregateId, Version)
        );
        */
    }
}
