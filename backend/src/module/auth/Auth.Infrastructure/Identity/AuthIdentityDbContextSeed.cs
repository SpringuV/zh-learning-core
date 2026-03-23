namespace Auth.Infrastructure.Identity;

public class AuthIdentityDbContextSeed
{
    public static async Task SeedAsync(AuthIdentityDbContext identityDbContext,
           UserManager<AuthApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (identityDbContext.Database.IsNpgsql())
        {
            await identityDbContext.Database.MigrateAsync();
        }

        if (!await roleManager.RoleExistsAsync(Constants.ADMINISTRATORS))
        {
            // create role "admin" if it doesn't exist
            await roleManager.CreateAsync(new IdentityRole(Constants.ADMINISTRATORS));
        }

        if (!await roleManager.RoleExistsAsync(Constants.USERS))
        {
            await roleManager.CreateAsync(new IdentityRole(Constants.USERS));
        }


        // create default admin if it doesn't exist
        const string adminUserName = "spring-admin@anhvu.com";
        if (await userManager.FindByNameAsync(adminUserName) == null)
        {
            var defaultAdmin = new AuthApplicationUser
            {
                UserName = adminUserName,
                Email = "xuanvuaudi2002@gmail.com"
            };
            var adminResult = await userManager.CreateAsync(defaultAdmin, Constants.DEFAULT_PASSWORD);
            if (!adminResult.Succeeded) throw new Exception(string.Join(", ", adminResult.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(defaultAdmin, Constants.ADMINISTRATORS);
        }

        // create default user if it doesn't exist
        const string defaultUserUserName = "spring-user@anhvu.com";
        if (await userManager.FindByNameAsync(defaultUserUserName) == null)
        {
            var defaultUser = new AuthApplicationUser
            {
                UserName = defaultUserUserName,
                Email = "tranxuanvu0210@gmail.com"
            };
            var userResult = await userManager.CreateAsync(defaultUser, Constants.DEFAULT_PASSWORD);
            if (!userResult.Succeeded) throw new Exception(string.Join(", ", userResult.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(defaultUser, Constants.USERS);
        }

    }
}
