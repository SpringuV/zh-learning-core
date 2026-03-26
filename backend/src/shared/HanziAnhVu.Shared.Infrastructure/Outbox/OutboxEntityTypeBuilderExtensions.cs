using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Infrastructure.Outbox;

/// <summary>
/// Provides extension methods for configuring outbox message entities in Entity Framework Core.
/// class OutboxEntityTypeBuilderExtensions contains an extension method ConfigureOutboxMessage
/// that configures the entity type for outbox messages, including table name, keys, indexes, and property mappings. 
/// This allows for consistent configuration of outbox message entities across different applications 
/// while adhering to the IOutboxMessage contract.
/// </summary>
public static class OutboxEntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TOutboxMessage> ConfigureOutboxMessage<TOutboxMessage>(
        this EntityTypeBuilder<TOutboxMessage> entity,
        string tableName)
        where TOutboxMessage : class, IOutboxMessage
    {
        entity.ToTable(tableName);
        entity.HasKey(m => m.Id);
        entity.HasIndex(m => m.ProcessedOnUtc);
        entity.HasIndex(m => m.OccurredOnUtc);
        entity.Property(m => m.Type).IsRequired();
        entity.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
        entity.Property(m => m.OccurredOnUtc).IsRequired();

        return entity;
    }
}
