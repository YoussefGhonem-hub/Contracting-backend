using Contracting.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Infrustructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IWebHostEnvironment env)
    {
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);

        //var adminId = await GetAdminUserIdAsync(userManager);
        //var customerId = await GetFirstUserIdInRoleAsync(userManager, "SuperAdmin");



    }

    private static async Task<Guid?> GetFirstUserIdInRoleAsync(UserManager<ApplicationUser> userManager, string role)
    {
        var users = await userManager.GetUsersInRoleAsync(role);
        return users.FirstOrDefault()?.Id;
    }

    // NEW: prefer the dedicated Admin user over any SuperAdmin (who is also in Admin)
    private static async Task<Guid?> GetAdminUserIdAsync(UserManager<ApplicationUser> userManager)
    {
        // Prefer the known seeded admin account
        var admin = await userManager.FindByEmailAsync("admin@shop.com");
        if (admin != null) return admin.Id;

        // Fallback to any user in Admin role excluding SuperAdmins
        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if (admins == null || admins.Count == 0) return null;

        var superAdmins = await userManager.GetUsersInRoleAsync("SuperAdmin");
        var superAdminIds = new HashSet<Guid>(superAdmins.Select(u => u.Id));

        var pureAdmin = admins.FirstOrDefault(u => !superAdminIds.Contains(u.Id));
        return pureAdmin?.Id ?? admins.First().Id;
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpperInvariant()
                });
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var superAdminEmail = "superadmin@shop.com";
        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdmin is null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                FullName = "Super Administrator",
                IsActive = true
            };
            if ((await userManager.CreateAsync(superAdmin, "SuperAdmin@123")).Succeeded)
                await userManager.AddToRolesAsync(superAdmin, new[] { "SuperAdmin", "Admin" });
        }

        var adminEmail = "admin@shop.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Admin",
                IsActive = true
            };
            if ((await userManager.CreateAsync(admin, "Admin@123")).Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        var customerEmail = "customer@shop.com";
        var customer = await userManager.FindByEmailAsync(customerEmail);
        if (customer is null)
        {
            customer = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                EmailConfirmed = true,
                FullName = "Demo Customer",
                IsActive = true
            };
            if ((await userManager.CreateAsync(customer, "Customer@123")).Succeeded)
                await userManager.AddToRoleAsync(customer, "Customer");
        }
    }
}