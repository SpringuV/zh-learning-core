using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Outbox;

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
            entity.ConfigureOutboxMessage("OutboxMessages");
        });
    }
}
