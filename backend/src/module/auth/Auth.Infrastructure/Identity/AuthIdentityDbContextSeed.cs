namespace Auth.Infrastructure.Identity;

public class AuthIdentityDbContextSeed
{
    public static async Task SeedAsync(AuthIdentityDbContext identityDbContext,
           UserManager<AuthApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (identityDbContext.Database.IsNpgsql())
        {
            // migrate này sẽ tạo ra bảng __EFMigrationsHistory nếu chưa tồn tại, sau đó mới áp dụng các migration còn thiếu
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
                Email = "test1@gmail.com",
            };
            var adminResult = await userManager.CreateAsync(defaultAdmin, Constants.DEFAULT_PASSWORD);
            if (!adminResult.Succeeded) throw new Exception(string.Join(", ", adminResult.Errors.Select(e => e.Description)));

            defaultAdmin.Activate(); // Call Activate() method instead of setting IsActive directly
            await identityDbContext.SaveChangesAsync(); // Save the activation 

            await userManager.AddToRoleAsync(defaultAdmin, Constants.ADMINISTRATORS);
        }

        // create default user if it doesn't exist
        const string defaultUserUserName = "spring-user@anhvu.com";
        if (await userManager.FindByNameAsync(defaultUserUserName) == null)
        {
            var defaultUser = new AuthApplicationUser
            {
                UserName = defaultUserUserName,
                Email = "test2@gmail.com"
            };
            var userResult = await userManager.CreateAsync(defaultUser, Constants.DEFAULT_PASSWORD);
            if (!userResult.Succeeded) throw new Exception(string.Join(", ", userResult.Errors.Select(e => e.Description)));

            defaultUser.Activate(); // Call Activate() method instead of setting IsActive directly
            await identityDbContext.SaveChangesAsync(); // Save the activation state

            await userManager.AddToRoleAsync(defaultUser, Constants.USERS);
        }

    }
}
