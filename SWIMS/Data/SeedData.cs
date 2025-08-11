// -------------------------------------------------------------------
// File:    SeedData.cs
// Author:  N/A
// Created: N/A
// Purpose: Seeds the database with default roles and an initial admin user.
// Dependencies:
//   - SwRole, SwUser (Identity entities)
//   - RoleManager<SwRole>, UserManager<SwUser> (Identity services)
//   - IConfiguration (for retrieving initial credentials)
// -------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SWIMS.Models;

namespace SWIMS.Data
{
    /// <summary>
    /// Provides methods to seed initial application data,
    /// including default roles and the admin user.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Ensures that default roles (e.g., <c>Admin</c>) and the initial admin account exist.
        /// Creates them if they do not.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceProvider"/> used to resolve
        /// <see cref="RoleManager{SwRole}"/>, <see cref="UserManager{SwUser}"/>,
        /// and <see cref="IConfiguration"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous seeding operation.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if creating the initial admin user fails, with detailed identity errors.
        /// </exception>
        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<SwRole>>();
            var userMgr = services.GetRequiredService<UserManager<SwUser>>();

            var config = services.GetRequiredService<IConfiguration>();
            var adminEmail = config["AdminUser:Email"]
                                ?? throw new InvalidOperationException("Missing AdminUser:Email in configuration");
            var adminPassword = config["AdminUser:Password"]
                                ?? throw new InvalidOperationException("Missing AdminUser:Password in configuration");


            // 1) Ensure roles exist
            foreach (var roleName in new[] { "Admin", "Basic" })
            {
                if (!await roleMgr.RoleExistsAsync(roleName))
                {
                    await roleMgr.CreateAsync(new SwRole { Name = roleName });
                }
            }

            // 2) Ensure initial admin user
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new SwUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin"
                };

                var result = await userMgr.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    throw new Exception("Failed to create initial admin user: " +
                        string.Join(", ", result.Errors));
                }
            }
            else
            {
                if (!await userMgr.IsInRoleAsync(admin, "Admin"))
                    await userMgr.AddToRoleAsync(admin, "Admin");
            }

        }
    }
}
