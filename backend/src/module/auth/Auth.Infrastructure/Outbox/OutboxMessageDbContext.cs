using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Outbox;

public class OutboxMessageDbContext : DbContext
{
    public OutboxMessageDbContext(DbContextOptions<OutboxMessageDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    
        builder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.ProcessedOnUtc);
            entity.HasIndex(m => m.OccurredOnUtc);
            entity.Property(m => m.Type).IsRequired();
            entity.Property(m => m.Payload).HasColumnType("jsonb").IsRequired();
            entity.Property(m => m.OccurredOnUtc).IsRequired();
        });
    }
}
