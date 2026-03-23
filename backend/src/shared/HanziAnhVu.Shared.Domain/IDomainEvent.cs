using System;
using System.Collections.Generic;
using System.Text;

namespace HanziAnhVu.Shared.Domain
{
    public interface IDomainEvent
    {
        // Để debug, log biết event xảy ra lúc nào
        DateTime OccurredAt { get; }

        // Để idempotency — tránh xử lý 2 lần cùng 1 event
        Guid EventId { get; }
    }
}
