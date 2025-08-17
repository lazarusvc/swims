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

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SWIMS.Models;
using SWIMS.Models.Security;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

            var db = services.GetRequiredService<SwimsIdentityDbContext>();


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


            // Helper to upsert a policy with role links
            async Task UpsertPolicyAsync(string policyName, params string[] roleNames)
            {
                var policy = await db.AuthorizationPolicies
                    .Include(p => p.Roles)
                    .FirstOrDefaultAsync(p => p.Name == policyName);

                if (policy == null)
                {
                    policy = new AuthorizationPolicyEntity
                    {
                        Name = policyName,
                        IsEnabled = true,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    db.AuthorizationPolicies.Add(policy);
                }

                // Clear and re-add to keep RoleName in sync (simple and safe)
                policy.Roles.Clear();
                foreach (var rn in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var role = await roleMgr.FindByNameAsync(rn);
                    if (role is null) continue; // role might not exist yet; skip silently or throw if you prefer

                    policy.Roles.Add(new AuthorizationPolicyRole
                    {
                        RoleId = role.Id,
                        Role = role,
                        RoleName = role.Name! // denormalized
                    });
                }
                policy.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }

            // Seed a couple of named policies
            await UpsertPolicyAsync("AdminOnly", "Admin");
            await UpsertPolicyAsync("ProgramManager", "Admin", "ProgramManager");

        }
    }
}
