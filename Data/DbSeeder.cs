
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndSuperAdmin(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                UserRoles.SuperAdmin,
                UserRoles.PlacementOfficer,
                UserRoles.CompanyHR,
                UserRoles.Student
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var email = configuration["SuperAdmin:Email"];
            var password = configuration["SuperAdmin:Password"];
            var fullName = configuration["SuperAdmin:FullName"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidOperationException(
                    "SuperAdmin configuration is missing.");
            }

            var superAdmin = await userManager.FindByEmailAsync(email);

            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    AccountStatus = AccountStatus.Approved,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(superAdmin, password);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(superAdmin, UserRoles.SuperAdmin))
            {
                await userManager.AddToRoleAsync(superAdmin, UserRoles.SuperAdmin);
            }
        }
    }
}
