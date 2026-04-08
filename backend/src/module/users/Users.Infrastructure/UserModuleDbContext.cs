using Users.Domain;
using Shared.Infrastructure.Outbox;

namespace Users.Infrastructure;

public class UserModuleDbContext(DbContextOptions<UserModuleDbContext> options) : DbContext(options)
{
    public DbSet<UserAggregate> Users { get; set; } = null!;
    public DbSet<UserModuleOutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserAggregate>(entity =>
        {   
            entity.ToTable("UserProfiles");
            entity.HasKey(u => u.Id);
            
            // Properties mapping
            entity.Property(u => u.Id);
            entity.Property(u => u.CurrentHskLevel).IsRequired();
            entity.Property(u => u.AiMessagesUsedToday).IsRequired();
            entity.Property(u => u.AiMessagesUsedTodayDate).IsRequired();
            entity.Property(u => u.AiUsageResetAt).IsRequired();
            entity.Property(u => u.SubscriptionStatus)
                .HasConversion<int>()  // Enum to int
                .IsRequired();
            entity.Property(u => u.CurrentTier)
                .HasConversion<int>()  // Enum to int
                .IsRequired();
            // Indexes
            entity.HasIndex(u => u.Id).IsUnique();
        });

        builder.Entity<UserModuleOutboxMessage>(entity =>
        {
            entity.ConfigureOutboxMessage("OutboxMessagesUsersModule");
        });
    }
}