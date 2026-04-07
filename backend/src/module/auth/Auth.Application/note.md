Flow Hiện Tại (Không dùng Aggregate)

    Handler
    ↓
    await \_publisher.Publish(DomainEvent) [In-process]
    ↓
    OutboxEvent Handler lắng nghe
    ↓
    outboxWriter.EnqueueAsync(IntegrationEvent) → Insert Outbox
    ↓
    Worker (NOTIFY) trigger
    ↓
    Export IntegrationEvent ra EventBus

Flow Mới (Dùng BaseAggregateRoot)

1. Handler load/create Aggregate
   ↓
2. Handler call aggregate.BusinessMethod()
   ↓
3. Aggregate.AddDomainEvent(event) → Stored in \_domainEvents list
   ↓
4. Handler save aggregate via Repository
   ↓
5. Repository intercept → Pop DomainEvents từ aggregate
   ↓
6. Repository persist aggregate + publish DomainEvents [In-process]
   ↓
7. OutboxEvent Handler lắng nghe (SAME as before)
   ↓
8. outboxWriter.EnqueueAsync(...) → Insert Outbox
   ↓
9. Worker (NOTIFY) trigger (SAME as before)
   ↓
10. Export IntegrationEvent ra EventBus

BEFORE (Current):
CreateCourse Command
→ IdentityService.RegisterUser()
→ \_publisher.Publish(DomainEvent) [Handler tự gọi]
→ OutboxHandler lắng nghe
→ Outbox table insert

AFTER (With Aggregate):
CreateCourse Command
→ CourseAggregate.Create()
→ AddDomainEvent() [Event stored in aggregate]
→ repository.SaveAsync(aggregate)
→ Save aggregate to DB
→ Extract & publish DomainEvents [trong Repository]
→ OutboxHandler lắng nghe [SAME]
→ Outbox table insert [SAME]
