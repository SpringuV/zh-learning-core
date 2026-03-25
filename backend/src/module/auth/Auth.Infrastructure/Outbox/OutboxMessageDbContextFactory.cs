using Auth.Infrastructure.DesignTime;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Infrastructure.Outbox;

public class OutboxMessageDbContextFactory : IDesignTimeDbContextFactory<OutboxMessageDbContext>
{
    public OutboxMessageDbContext CreateDbContext(string[] args)
    {
        return new OutboxMessageDbContext(
            DesignTimeDbContextOptionsFactory.CreateOptions<OutboxMessageDbContext>());
    }
}
