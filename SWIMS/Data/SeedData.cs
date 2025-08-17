// -------------------------------------------------------------------
// File:    SeedData.cs
// Purpose: Seeds default roles, an initial admin user, and DB-backed
//          authorization policies (with IsSystem support).
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SWIMS.Models;
using SWIMS.Models.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWIMS.Data
{
    /// <summary>
    /// Provides methods to seed initial application data,
    /// including default roles, the admin user, and auth policies.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Ensures roles, initial admin user, and DB-backed policies exist.
        /// </summary>
        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<SwRole>>();
            var userMgr = services.GetRequiredService<UserManager<SwUser>>();
            var config = services.GetRequiredService<IConfiguration>();
            var db = services.GetRequiredService<SwimsIdentityDbContext>();

            var adminEmail = config["AdminUser:Email"]
                                ?? throw new InvalidOperationException("Missing AdminUser:Email in configuration");
            var adminPassword = config["AdminUser:Password"]
                                ?? throw new InvalidOperationException("Missing AdminUser:Password in configuration");

            // 1) Ensure required roles exist
            foreach (var roleName in new[] { "Admin", "Basic", "ProgramManager" })
            {
                if (!await roleMgr.RoleExistsAsync(roleName))
                {
                    var createRes = await roleMgr.CreateAsync(new SwRole { Name = roleName });
                    if (!createRes.Succeeded)
                        throw new Exception($"Failed creating role '{roleName}': " +
                            string.Join(", ", createRes.Errors.Select(e => $"{e.Code}:{e.Description}")));
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
                if (!result.Succeeded)
                    throw new Exception("Failed to create initial admin user: " +
                        string.Join(", ", result.Errors.Select(e => $"{e.Code}:{e.Description}")));

                await userMgr.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                if (!await userMgr.IsInRoleAsync(admin, "Admin"))
                    await userMgr.AddToRoleAsync(admin, "Admin");
            }

            // 3) Policies: upsert helper (supports IsSystem + Description)
            async Task UpsertPolicyAsync(string policyName, string[] roleNames, bool isSystem = false, string? description = null)
            {
                var policy = await db.AuthorizationPolicies
                    .Include(p => p.Roles)
                    .FirstOrDefaultAsync(p => p.Name == policyName);

                if (policy == null)
                {
                    policy = new AuthorizationPolicyEntity
                    {
                        Name = policyName,
                        Description = description,
                        IsEnabled = true,              // new policies start enabled
                        IsSystem = isSystem,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    db.AuthorizationPolicies.Add(policy);
                }
                else
                {
                    // Keep authoritative flags/descriptions from seed for system policies
                    policy.Description = description ?? policy.Description;
                    policy.IsSystem = isSystem || policy.IsSystem;

                    // >>> Force-enable AdminOnly if it already exists (requested change)
                    if (string.Equals(policyName, "AdminOnly", StringComparison.OrdinalIgnoreCase))
                        policy.IsEnabled = true;

                    policy.UpdatedAt = DateTimeOffset.UtcNow;

                    // Rebuild role links to keep RoleName in sync
                    policy.Roles.Clear();
                }

                foreach (var rn in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var role = await roleMgr.FindByNameAsync(rn);
                    if (role is null) continue; // role should exist from step 1

                    policy.Roles.Add(new AuthorizationPolicyRole
                    {
                        RoleId = role.Id,
                        Role = role,
                        RoleName = role.Name! // denormalized for fast RequireRole()
                    });
                }

                await db.SaveChangesAsync();
            }

            // 4) Seed named policies
            await UpsertPolicyAsync(
                policyName: "AdminOnly",
                roleNames: new[] { "Admin" },
                isSystem: true,
                description: "Core admin access; protects administrative features"
            );

            await UpsertPolicyAsync(
                policyName: "ProgramManager",
                roleNames: new[] { "Admin", "ProgramManager" },
                isSystem: false,
                description: "Access for program management features"
            );
        }
    }
}
