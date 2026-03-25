using Auth.Infrastructure.DesignTime;
using Microsoft.EntityFrameworkCore.Design;
namespace Auth.Infrastructure.Identity;

public class AuthIdentityDbContextFactory
    : IDesignTimeDbContextFactory<AuthIdentityDbContext>
{
    public AuthIdentityDbContext CreateDbContext(string[] args)
    {
        return new AuthIdentityDbContext(
            DesignTimeDbContextOptionsFactory.CreateOptions<AuthIdentityDbContext>());
    }
}
