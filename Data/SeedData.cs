using DotNetEnv;
using Microsoft.AspNetCore.Identity;

namespace BlazorShortUrl.Data;

public enum Roles : ushort
{
    Admin = 0,
    Basic = 1
}

// Seed Identity database
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        if (context == null || context.ApplicationUser == null || context.ApplicationRole == null)
        {
            throw new NullReferenceException("Null DbContext or DbSet");
        }

        // Exit if data already exists
        if (context.ApplicationUser.Any() || context.ApplicationRole.Any())
        {
            return;
        }

        await SeedData.SeedRolesAsync(userManager, roleManager);
        await SeedData.SeedAdminAsync(userManager, roleManager);
    }

    private static async Task SeedRolesAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        // Seed Roles
        await roleManager.CreateAsync(new AppRole(Roles.Admin.ToString()));
        await roleManager.CreateAsync(new AppRole(Roles.Basic.ToString()));
    }

    private static async Task SeedAdminAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        // Seed default admin user
        var defaultUser = new AppUser
        {
            UserName = Env.GetString("ADMIN_EMAIL"),
            Email = Env.GetString("ADMIN_EMAIL"),
            EmailConfirmed = true
        };
        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, Env.GetString("ADMIN_PASSWORD"));
                await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
            }
        }
    }
}