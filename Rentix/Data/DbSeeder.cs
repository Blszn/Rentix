using Microsoft.AspNetCore.Identity;
using Rentix.Models;

namespace Rentix.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // UserManager ve RoleManager çağırma
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // Rolleri Ekle
            await CreateRoleAsync(roleManager, "Admin");
            await CreateRoleAsync(roleManager, "User");

            // Admin Kullanıcısını Ekle
            var adminEmail = "admin@rentix.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin@rentix.local",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}