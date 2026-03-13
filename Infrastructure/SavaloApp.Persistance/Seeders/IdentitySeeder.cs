using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        const string adminRole = "Admin";
        const string adminEmail = "admin@savaloapp.com";
        const string adminPassword = "Admin123!";
        const string firstName = "Super";
        const string lastName = "Admin";
        const string phoneNumber = "+994501111111";

        // Role yarat
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
                throw new Exception($"ADMIN_ROLE_CREATE_FAILED: {errors}");
            }
        }

        // User var?
        var existingUser = await userManager.FindByEmailAsync(adminEmail);

        if (existingUser == null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = $"{firstName} {lastName}".Trim(),
                AccountType = "basic",
                Language = "az",
                TimeZone = "Asia/Baku",
                PhoneNumber = phoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var userResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (!userResult.Succeeded)
            {
                var errors = string.Join(", ", userResult.Errors.Select(x => x.Description));
                throw new Exception($"ADMIN_USER_CREATE_FAILED: {errors}");
            }

            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole);

            if (!addToRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addToRoleResult.Errors.Select(x => x.Description));
                throw new Exception($"ADMIN_ROLE_ASSIGN_FAILED: {errors}");
            }
        }
        else
        {
            var roles = await userManager.GetRolesAsync(existingUser);
            if (!roles.Contains(adminRole))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(existingUser, adminRole);

                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(x => x.Description));
                    throw new Exception($"ADMIN_ROLE_ASSIGN_FAILED: {errors}");
                }
            }
        }
    }
}