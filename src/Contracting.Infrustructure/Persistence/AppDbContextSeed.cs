using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        await SeedUsersAsync(userManager, context);
        await SeedAccountsUserAsync(userManager);
        await SeedClientUserAsync(userManager);

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
        string[] roles = { "SuperAdmin", "Admin", "Teamlead-engineer", "Site-engineer", "Office-engineer", "Accounts", "IT", "Viewer", "Client" };
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

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
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
            {
                await userManager.AddToRolesAsync(superAdmin, new[] { "SuperAdmin", "Admin" });
                await SeedEngineerForUserAsync(context, superAdmin, "Super Administrator", "superAdminEngineer");
            }
        }
        else
        {
            await SeedEngineerForUserAsync(context, superAdmin, "Super Administrator", "superAdminEngineer");
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
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                await SeedEngineerForUserAsync(context, admin, "System Admin", "systemAdminEngineer");
            }
        }
        else
        {
            await SeedEngineerForUserAsync(context, admin, "System Admin", "systemAdminEngineer");
        }
    }

    private static async Task SeedClientUserAsync(UserManager<ApplicationUser> userManager)
    {
        var clientEmail = "client@shop.com";
        var client = await userManager.FindByEmailAsync(clientEmail);
        if (client is null)
        {
            client = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                EmailConfirmed = true,
                FullName = "Default Client",
                IsActive = true
            };
            if ((await userManager.CreateAsync(client, "Client@123")).Succeeded)
            {
                await userManager.AddToRoleAsync(client, "Client");
            }
        }
    }

    private static async Task SeedAccountsUserAsync(UserManager<ApplicationUser> userManager)
    {
        var accountsEmail = "accounts@shop.com";
        var accountsUser = await userManager.FindByEmailAsync(accountsEmail);
        if (accountsUser is null)
        {
            accountsUser = new ApplicationUser
            {
                UserName = accountsEmail,
                Email = accountsEmail,
                EmailConfirmed = true,
                FullName = "Default Accounts",
                IsActive = true
            };

            if ((await userManager.CreateAsync(accountsUser, "Accounts@123")).Succeeded)
            {
                await userManager.AddToRoleAsync(accountsUser, "Accounts");
            }
        }
    }

    private static async Task SeedEngineerForUserAsync(
        ApplicationDbContext context,
        ApplicationUser user,
        string nameEn,
        string position)
    {
        bool engineerExists = await context.Set<Engineer>()
            .AnyAsync(e => e.ApplicationUserId == user.Id);

        if (!engineerExists)
        {
            var engineer = new Engineer
            {
                nameEn = nameEn,
                Email = user.Email,
                phoneNumber = user.PhoneNumber,
                position = position,
                ApplicationUserId = user.Id,
            };

            await context.Set<Engineer>().AddAsync(engineer);
            await context.SaveChangesAsync();
        }
    }
}