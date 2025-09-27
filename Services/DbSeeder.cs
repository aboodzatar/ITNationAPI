using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using iTeamAPI.Models;

public static class DbSeeder
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create admin user if not exists
        var adminUser = await userManager.FindByNameAsync("aboodzatar292");
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "aboodzatar292",
                FullName = "Abood Zatar",
            };

            var result = await userManager.CreateAsync(user, "AbooDzatar");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
