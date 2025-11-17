// -------------------------------------------------------------------
// File:    SeedData.cs
// Purpose: Seeds default roles, an initial admin user, and DB-backed
//          authorization policies (with IsSystem support).
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using SWIMS.Models;
using SWIMS.Models.Security;
using SWIMS.Security;
using System;
using System.Linq;
using System.Security.Claims;
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

            // 0) Helpers
            async Task EnsureRoleAsync(string name)
            {
                if (!await roleMgr.RoleExistsAsync(name))
                {
                    var res = await roleMgr.CreateAsync(new SwRole { Name = name });
                    if (!res.Succeeded)
                        throw new Exception($"Failed creating role '{name}': " +
                            string.Join(", ", res.Errors.Select(e => $"{e.Code}:{e.Description}")));
                }
            }

            async Task EnsureRoleHasClaimAsync(string roleName, string type, string value)
            {
                var role = await roleMgr.FindByNameAsync(roleName);
                if (role is null) return;
                var claims = await roleMgr.GetClaimsAsync(role);
                if (!claims.Any(c => c.Type == type && c.Value == value))
                    await roleMgr.AddClaimAsync(role, new Claim(type, value));
            }

            // 1) MAIN ROLES (gov context from FRD Appendix I)
            string[] mainRoles = new[]
            {
                "SuperAdmin",               // break-glass (full system control)
                "Admin",                    // system administrator/developer
                "ProgramManager",           // mgmt bucket (reports etc.)
                "SocialWorker",
                "Secretary",
                "VCCClerk",
                "Coordinator",
                "Director",
                "PermanentSecretary",
                "Minister",
                "SEOAAccounts",
                "VotesClerk",
                "ReportManager",
                "Auditor",
                "ReadOnly"
            };

            foreach (var r in mainRoles) await EnsureRoleAsync(r);

            // 2) INITIAL LOCAL USERS: SuperAdmin + Admin (kept separate from LDAP population)

            // --- SuperAdmin (break-glass) ---
            var superEmail = config["SuperAdminUser:Email"]
                ?? throw new InvalidOperationException("Missing SuperAdminUser:Email in configuration");
            var superPassword = config["SuperAdminUser:Password"]
                ?? throw new InvalidOperationException("Missing SuperAdminUser:Password in configuration");

            var super = await userMgr.FindByEmailAsync(superEmail);
            if (super == null)
            {
                super = new SwUser
                {
                    UserName = superEmail,
                    Email = superEmail,
                    FirstName = "System",
                    LastName = "SuperAdmin"
                };
                var create = await userMgr.CreateAsync(super, superPassword);
                if (!create.Succeeded)
                    throw new Exception("Failed to create SuperAdmin user: " +
                        string.Join(", ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
                var token = await userMgr.GenerateEmailConfirmationTokenAsync(super);
                var confirm = await userMgr.ConfirmEmailAsync(super, token);
                if (!confirm.Succeeded)
                    throw new Exception("Failed to confirm SuperAdmin email: " +
                        string.Join(", ", confirm.Errors.Select(e => $"{e.Code}:{e.Description}")));
            }
            // SuperAdmin must have both roles to cover admin UX and policy screens:
            foreach (var r in new[] { "SuperAdmin", "Admin" })
                if (!await userMgr.IsInRoleAsync(super, r)) await userMgr.AddToRoleAsync(super, r);

            // --- Admin (normal local admin—distinct from SuperAdmin) ---
            var adminEmail = config["AdminUser:Email"]
                ?? throw new InvalidOperationException("Missing AdminUser:Email in configuration");
            var adminPassword = config["AdminUser:Password"]
                ?? throw new InvalidOperationException("Missing AdminUser:Password in configuration");

            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new SwUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin"
                };
                var createAdmin = await userMgr.CreateAsync(admin, adminPassword);
                if (!createAdmin.Succeeded)
                    throw new Exception("Failed to create Admin user: " +
                        string.Join(", ", createAdmin.Errors.Select(e => $"{e.Code}:{e.Description}")));
                var tokenAdmin = await userMgr.GenerateEmailConfirmationTokenAsync(admin);
                var confirmAdmin = await userMgr.ConfirmEmailAsync(admin, tokenAdmin);
                if (!confirmAdmin.Succeeded)
                    throw new Exception("Failed to confirm Admin email: " +
                        string.Join(", ", confirmAdmin.Errors.Select(e => $"{e.Code}:{e.Description}")));
            }
            // Admin should be in Admin role only (not SuperAdmin):
            if (!await userMgr.IsInRoleAsync(admin, "Admin"))
                await userMgr.AddToRoleAsync(admin, "Admin");


            // 3) SUBROLES (Perm:<Permission>) + attach "permission" claim to each
            string[] allPermissions =
            [
                // Reporting / Forms (yours already existed)
                Permissions.Reports_View, Permissions.Reports_Admin, Permissions.Forms_Builder, Permissions.Forms_Submit,
                Permissions.Programs_View, Permissions.Forms_Manage,

                // NEW: Intake/Clients/Applications/Assessment
                Permissions.Intake_View, Permissions.Intake_Create, Permissions.Intake_Edit, Permissions.Intake_Assign,
                Permissions.Clients_View, Permissions.Clients_Create, Permissions.Clients_Edit, Permissions.Clients_Archive,
                Permissions.Applications_View, Permissions.Applications_Edit, Permissions.Applications_Assign,
                Permissions.Assessment_View, Permissions.Approvals_L1, Permissions.Approvals_L2, Permissions.Approvals_L3,
                Permissions.Approvals_L4, Permissions.Approvals_L5,

                // Payments
                Permissions.Payments_View, Permissions.Payments_Validate, Permissions.Payments_ExportLists, Permissions.Payments_Reconcile,

                // Reference data & SPs
                Permissions.RefData_Manage, Permissions.SP_Run, Permissions.SP_Manage, Permissions.SP_Params,

                // Admin and API
                Permissions.Admin_Users, Permissions.Admin_Roles, Permissions.Admin_Settings,
                Permissions.Admin_Policies, Permissions.Admin_Endpoints, Permissions.Admin_PublicEndpoints,
                Permissions.Admin_RouteInspector, Permissions.Admin_AuditLogs,
                Permissions.Admin_Hangfire, Permissions.Admin_ApiDashboard, Permissions.Admin_SessionLog,
                Permissions.Api_Access
            ];

            foreach (var p in allPermissions)
            {
                var subRole = $"Perm:{p}";
                await EnsureRoleAsync(subRole);
                await EnsureRoleHasClaimAsync(subRole, "permission", p);
            }

            // 4) POLICY UPSERT (role-OR per permission; always include SuperAdmin)
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
                        IsEnabled = true,
                        IsSystem = isSystem,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    db.AuthorizationPolicies.Add(policy);
                }
                else
                {
                    policy.Description = description ?? policy.Description;
                    policy.IsSystem = isSystem || policy.IsSystem;
                    policy.IsEnabled = true; // keep enabled on seed; Admin can disable via UI
                    policy.UpdatedAt = DateTimeOffset.UtcNow;
                }

                foreach (var rn in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var role = await roleMgr.FindByNameAsync(rn);
                    if (role is null) continue;

                    var hasRole = policy.Roles.Any(x => x.RoleName.Equals(rn, StringComparison.OrdinalIgnoreCase));
                    if (!hasRole)
                    {
                        policy.Roles.Add(new AuthorizationPolicyRole
                        {
                            RoleId = role.Id,
                            Role = role,
                            RoleName = role.Name!
                        });
                    }
                }
                await db.SaveChangesAsync();
            }

            // 5) POLICY MATRIX (derived from FRD Roles/Permissions; SuperAdmin everywhere)
            string[] Always(string perm, params string[] roles)
                => roles.Concat(new[] { $"Perm:{perm}", "SuperAdmin" }).ToArray();

            // Admin/system policies
            await UpsertPolicyAsync("SuperAdminOnly", new[] { "SuperAdmin" }, isSystem: true, description: "Break-glass access");
            await UpsertPolicyAsync("AdminOnly", new[] { "SuperAdmin", "Admin" }, isSystem: true, description: "Administrative access");

            // Reports
            await UpsertPolicyAsync(Permissions.Reports_View,
                Always(Permissions.Reports_View, "Admin", "ProgramManager", "ReportManager", "Director", "PermanentSecretary", "Minister","ReadOnly"),
                description: "View analytics & dashboards");
            await UpsertPolicyAsync(Permissions.Reports_Admin,
                Always(Permissions.Reports_Admin, "Admin", "ReportManager"),
                isSystem: true, description: "Manage report defs & params");

            // Forms / Form Builder
            await UpsertPolicyAsync(Permissions.Programs_View,
                Always(Permissions.Programs_View,
                       "SocialWorker", "Secretary", "Coordinator", "Director", "PermanentSecretary", "Minister", "ProgramManager", "ReadOnly"),
                description: "View Programs dashboard");
            await UpsertPolicyAsync(Permissions.Forms_Manage,
                Always(Permissions.Forms_Manage, "Admin"),
                isSystem: true, description: "Manage forms metadata & processes");
            await UpsertPolicyAsync(Permissions.Forms_Builder,
                Always(Permissions.Forms_Builder, "Admin"),
                isSystem: true, description: "Create/edit forms");
            await UpsertPolicyAsync(Permissions.Forms_Submit,
                Always(Permissions.Forms_Submit, "SocialWorker", "Secretary", "VCCClerk"),
                description: "Submit operational forms");

            // Intake
            await UpsertPolicyAsync(Permissions.Intake_View,
                Always(Permissions.Intake_View, "SocialWorker", "Secretary", "VCCClerk", "Coordinator","ReadOnly"),
                description: "View intake records");
            await UpsertPolicyAsync(Permissions.Intake_Create,
                Always(Permissions.Intake_Create, "Secretary", "VCCClerk"),
                description: "Create intake records");
            await UpsertPolicyAsync(Permissions.Intake_Edit,
                Always(Permissions.Intake_Edit, "Secretary", "VCCClerk", "SocialWorker"),
                description: "Edit intake records");
            await UpsertPolicyAsync(Permissions.Intake_Assign,
                Always(Permissions.Intake_Assign, "Secretary", "Coordinator"),
                description: "Assign intakes to workers");

            // Clients
            await UpsertPolicyAsync(Permissions.Clients_View,
                Always(Permissions.Clients_View, "SocialWorker", "VCCClerk", "Coordinator", "Director", "ReadOnly"),
                description: "View clients registry");
            await UpsertPolicyAsync(Permissions.Clients_Create,
                Always(Permissions.Clients_Create, "SocialWorker"),
                description: "Create client profiles");
            await UpsertPolicyAsync(Permissions.Clients_Edit,
                Always(Permissions.Clients_Edit, "SocialWorker", "VCCClerk"),
                description: "Edit client profiles");
            await UpsertPolicyAsync(Permissions.Clients_Archive,
                Always(Permissions.Clients_Archive, "Coordinator", "Director", "Admin"),
                description: "Archive/deactivate client records");

            // Applications
            await UpsertPolicyAsync(Permissions.Applications_View,
                Always(Permissions.Applications_View, "SocialWorker", "Secretary", "Coordinator", "ReadOnly"),
                description: "View applications");
            await UpsertPolicyAsync(Permissions.Applications_Edit,
                Always(Permissions.Applications_Edit, "SocialWorker"),
                description: "Edit application details");
            await UpsertPolicyAsync(Permissions.Applications_Assign,
                Always(Permissions.Applications_Assign, "Secretary", "Coordinator"),
                description: "Assign applications");

            // Assessment & approvals (5 levels: SW, Coord, Dir, PS, Minister)
            await UpsertPolicyAsync(Permissions.Assessment_View,
                Always(Permissions.Assessment_View, "SocialWorker", "Coordinator", "Director", "PermanentSecretary", "Minister", "ReadOnly"),
                description: "View assessment summary & forms");
            await UpsertPolicyAsync(Permissions.Approvals_L1,
                Always(Permissions.Approvals_L1, "SocialWorker"), description: "SW recommendation");
            await UpsertPolicyAsync(Permissions.Approvals_L2,
                Always(Permissions.Approvals_L2, "Coordinator"), description: "Coordinator approval");
            await UpsertPolicyAsync(Permissions.Approvals_L3,
                Always(Permissions.Approvals_L3, "Director"), description: "Director approval");
            await UpsertPolicyAsync(Permissions.Approvals_L4,
                Always(Permissions.Approvals_L4, "PermanentSecretary"), description: "PS approval");
            await UpsertPolicyAsync(Permissions.Approvals_L5,
                Always(Permissions.Approvals_L5, "Minister"), description: "Minister approval");

            // Payments
            await UpsertPolicyAsync(Permissions.Payments_View,
                Always(Permissions.Payments_View, "SEOAAccounts", "VotesClerk", "VCCClerk", "Admin", "ReadOnly"),
                description: "View payment processing data");
            await UpsertPolicyAsync(Permissions.Payments_Validate,
                Always(Permissions.Payments_Validate, "SEOAAccounts"),
                description: "Validate cases for payment");
            await UpsertPolicyAsync(Permissions.Payments_ExportLists,
                Always(Permissions.Payments_ExportLists, "VotesClerk"),
                description: "Generate Welfare 6/7 & export lists");
            await UpsertPolicyAsync(Permissions.Payments_Reconcile,
                Always(Permissions.Payments_Reconcile, "VotesClerk", "SEOAAccounts"),
                description: "Track/record reconciliation");

            // Reference data
            await UpsertPolicyAsync(Permissions.RefData_Manage,
                Always(Permissions.RefData_Manage, "Admin"),
                description: "Manage reference data (Beneficiary/City/Org/Financial Institution)");

            // Stored procedures (Ops)
            await UpsertPolicyAsync(Permissions.SP_Run,
                Always(Permissions.SP_Run, "Admin", "SEOAAccounts"),
                description: "Run operational stored procedures");
            await UpsertPolicyAsync(Permissions.SP_Manage,
                Always(Permissions.SP_Manage, "Admin"),
                description: "Manage stored procedures defs");
            await UpsertPolicyAsync(Permissions.SP_Params,
                Always(Permissions.SP_Params, "Admin"),
                description: "Manage stored procedure parameters");

            // Admin & API
            await UpsertPolicyAsync(Permissions.Admin_Users, Always(Permissions.Admin_Users, "Admin"), isSystem: true, description: "User admin");
            await UpsertPolicyAsync(Permissions.Admin_Roles, Always(Permissions.Admin_Roles, "Admin"), isSystem: true, description: "Role admin");
            await UpsertPolicyAsync(Permissions.Admin_Settings, Always(Permissions.Admin_Settings, "Admin"), isSystem: true, description: "System settings");
            await UpsertPolicyAsync(Permissions.Admin_Policies, Always(Permissions.Admin_Policies, "Admin"), isSystem: true, description: "Auth policies admin");
            await UpsertPolicyAsync(Permissions.Admin_Endpoints, Always(Permissions.Admin_Endpoints, "Admin"), isSystem: true, description: "Endpoint policy assignments");
            await UpsertPolicyAsync(Permissions.Admin_PublicEndpoints, Always(Permissions.Admin_PublicEndpoints, "Admin"), isSystem: true, description: "Public endpoints registry");
            await UpsertPolicyAsync(Permissions.Admin_RouteInspector, Always(Permissions.Admin_RouteInspector, "Admin"), isSystem: true, description: "Route inspector");
            await UpsertPolicyAsync(Permissions.Admin_Hangfire,
                Always(Permissions.Admin_Hangfire, "Admin"),
                isSystem: true, description: "Access Hangfire dashboard");
            await UpsertPolicyAsync(Permissions.Admin_SessionLog,
                Always(Permissions.Admin_SessionLog, "Admin","Auditor"),
                isSystem: true, description: "View sessions & admin activity");
            await UpsertPolicyAsync(Permissions.Admin_AuditLogs, Always(Permissions.Admin_AuditLogs, "Admin","Auditor"), isSystem: true, description: "Audit logs");

            // API
            await UpsertPolicyAsync(Permissions.Admin_ApiDashboard,
                Always(Permissions.Admin_ApiDashboard, "Admin"),
                isSystem: true, description: "Access API dashboard");
            await UpsertPolicyAsync(Permissions.Api_Access,
                Always(Permissions.Api_Access, "Admin"),
                description: "Programmatic access to API");

            // 6) Legacy policy synonyms (back-compat while we swap to dot-names)
            await UpsertPolicyAsync("ReportsView", new[] { "SuperAdmin", "Admin", "ProgramManager", "ReportManager", "Director", "PermanentSecretary", "Minister" });
            await UpsertPolicyAsync("ReportsAdmin", new[] { "SuperAdmin", "Admin", "ReportManager" });

            // 7) Public endpoints (keep yours + Identity UI; add as needed)
            async Task UpsertPublicEndpointAsync(string matchType, string? area = null, string? controller = null, string? action = null, string? page = null, string? path = null, string? regex = null, int priority = 100, string? notes = null)
            {
                var row = await db.PublicEndpoints.FirstOrDefaultAsync(x =>
                    x.MatchType == matchType
                    && (x.Area ?? "") == (area ?? "")
                    && (x.Controller ?? "") == (controller ?? "")
                    && (x.Action ?? "") == (action ?? "")
                    && (x.Page ?? "") == (page ?? "")
                    && (x.Path ?? "") == (path ?? "")
                    && (x.Regex ?? "") == (regex ?? "")
                );

                if (row == null)
                {
                    row = new PublicEndpoint
                    {
                        MatchType = matchType,
                        Area = area,
                        Controller = controller,
                        Action = action,
                        Page = page,
                        Path = path,
                        Regex = regex,
                        Priority = priority,
                        IsEnabled = true,
                        Notes = notes,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    db.PublicEndpoints.Add(row);
                }
                else
                {
                    row.Priority = priority; row.IsEnabled = true; row.Notes = notes ?? row.Notes; row.UpdatedAt = DateTimeOffset.UtcNow;
                }
                await db.SaveChangesAsync();
            }

            // default public endpoints...(MANDATORY)
            await UpsertPublicEndpointAsync(MatchTypes.ControllerAction, area: null, controller: "Home", action: "Index", priority: 10, notes: "Home page public by default");
            await UpsertPublicEndpointAsync(MatchTypes.ControllerAction, area: null, controller: "Home", action: "Privacy", priority: 10, notes: "Privacy page public");

            // Identity UI public pages (Razor Pages under Area = "Identity")
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/Login", priority: 5, notes: "Identity Login");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/Register", priority: 5, notes: "Identity Register");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/ForgotPassword", priority: 5, notes: "Forgot Password");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/ResetPassword", priority: 5, notes: "Reset Password");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/ConfirmEmail", priority: 5, notes: "Confirm Email");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/ExternalLogin", priority: 5, notes: "External Login");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/ExternalLoginCallback", priority: 5, notes: "External Login Callback");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/Lockout", priority: 5, notes: "Lockout");
            await UpsertPublicEndpointAsync(MatchTypes.RazorPage, area: "Identity", page: "/Account/AccessDenied", priority: 5, notes: "Access Denied (optional)");


            // --- Endpoint Policy Assignments (protected endpoints) ---
            async Task UpsertEndpointPolicyAsync(
                string policyName,
                string matchType,
                string? area = null,
                string? controller = null,
                string? action = null,
                string? page = null,
                string? path = null,
                string? regex = null,
                int priority = 100,
                string? notes = null)
            {
                var pol = await db.AuthorizationPolicies.FirstOrDefaultAsync(p => p.Name == policyName);
                if (pol is null) throw new InvalidOperationException($"Policy '{policyName}' not found.");

                var row = await db.EndpointPolicyAssignments.FirstOrDefaultAsync(x =>
                    x.MatchType == matchType &&
                    (x.Area ?? "") == (area ?? "") &&
                    (x.Controller ?? "") == (controller ?? "") &&
                    (x.Action ?? "") == (action ?? "") &&
                    (x.Page ?? "") == (page ?? "") &&
                    (x.Path ?? "") == (path ?? "") &&
                    (x.Regex ?? "") == (regex ?? ""));

                if (row == null)
                {
                    row = new EndpointPolicyAssignment
                    {
                        MatchType = matchType,
                        Area = area,
                        Controller = controller,
                        Action = action,
                        Page = page,
                        Path = path,
                        Regex = regex,
                        Notes = notes,
                        PolicyId = pol.Id,
                        PolicyName = pol.Name,
                        IsEnabled = true,
                        Priority = priority,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    db.EndpointPolicyAssignments.Add(row);
                }
                else
                {
                    // additive: keep existing, just ensure policy/flags
                    row.PolicyId = pol.Id;
                    row.PolicyName = pol.Name;
                    row.IsEnabled = true;
                    row.Priority = priority;
                    row.Notes = notes ?? row.Notes;
                    row.UpdatedAt = DateTimeOffset.UtcNow;
                }
                await db.SaveChangesAsync();
            }

            // --- Admin & admin-ish areas ---

            // Users & Roles admin UIs
            await UpsertEndpointPolicyAsync(Permissions.Admin_Users, MatchTypes.Controller, controller: "users", priority: 100, notes: "Admin: user management");
            await UpsertEndpointPolicyAsync(Permissions.Admin_Roles, MatchTypes.Controller, controller: "roles", priority: 100, notes: "Admin: role management");

            // Admin settings (future pages)
            await UpsertEndpointPolicyAsync(Permissions.Admin_Settings, MatchTypes.Controller, controller: "SystemSettings", priority: 100, notes: "Admin: system settings");

            // Authorization module (policies, endpoints, public endpoints, route inspector)
            await UpsertEndpointPolicyAsync(Permissions.Admin_Policies, MatchTypes.Controller, controller: "AuthorizationPolicies", priority: 100, notes: "Admin: auth policies");
            await UpsertEndpointPolicyAsync(Permissions.Admin_Endpoints, MatchTypes.Controller, controller: "EndpointPolicies", priority: 100, notes: "Admin: endpoint policies");
            await UpsertEndpointPolicyAsync(Permissions.Admin_PublicEndpoints, MatchTypes.Controller, controller: "PublicEndpoints", priority: 100, notes: "Admin: public endpoints");
            await UpsertEndpointPolicyAsync(Permissions.Admin_RouteInspector, MatchTypes.Controller, controller: "RouteInspector", priority: 100, notes: "Admin: route inspector");

            // Audit & sessions
            await UpsertEndpointPolicyAsync(Permissions.Admin_AuditLogs, MatchTypes.RazorPage, area: "Portal", page: "/Pages/Logs/Audit", priority: 120, notes: "Admin: audit logs");
            await UpsertEndpointPolicyAsync(Permissions.Admin_SessionLog, MatchTypes.RazorPage, area: "Portal", page: "/Pages/Logs/Sessions", priority: 120, notes: "Admin: sessions");

            // Hangfire dashboard (path-based since it isn’t an MVC controller)
            await UpsertEndpointPolicyAsync(Permissions.Admin_Hangfire, MatchTypes.Path, path: "/ops/hangfire", priority: 200, notes: "Admin: Hangfire dashboard");

            // API dashboard (use your actual path—example shown)
            await UpsertEndpointPolicyAsync(Permissions.Admin_ApiDashboard, MatchTypes.Controller, controller: "Api", priority: 100, notes: "Admin: API dashboard");

            // Stored Procedure module (double auth model: main gate here; later per-SP grants)
            await UpsertEndpointPolicyAsync(Permissions.SP_Manage, MatchTypes.Controller, controller: "StoredProcessesAdmin", priority: 100, notes: "SP: manage definitions");
            await UpsertEndpointPolicyAsync(Permissions.SP_Params, MatchTypes.Controller, controller: "StoredProcessParams", priority: 100, notes: "SP: manage params");
            await UpsertEndpointPolicyAsync(Permissions.SP_Run, MatchTypes.Controller, controller: "StoredProcesses", priority: 100, notes: "SP: run procedures");

            // Reports MVC + SSRS proxy (already seeded policy; add endpoint mapping so DB is SoT)
            await UpsertEndpointPolicyAsync(Permissions.Reports_View, MatchTypes.Controller, controller: "Reports", priority: 100, notes: "Reports UI");
            await UpsertEndpointPolicyAsync(Permissions.Reports_View, MatchTypes.Path, path: "/ssrs/*", priority: 200, notes: "SSRS proxy");

            // Default API gate (low precedence; specific rules beat it)
            await UpsertEndpointPolicyAsync(Permissions.Api_Access, MatchTypes.Path, path: "/api/v1/*", priority: 900, notes: "Default API gate");
            // Default UIs that use API system APIs
            await UpsertEndpointPolicyAsync(Permissions.RefData_Manage, MatchTypes.Controller, controller: "city", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.RefData_Manage, MatchTypes.Controller, controller: "beneficiary", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.RefData_Manage, MatchTypes.Controller, controller: "organization", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.RefData_Manage, MatchTypes.Controller, controller: "financial_institution", priority: 100);



            // --- Core app surfaces ---

            // Programs dashboard (overview page)
            await UpsertEndpointPolicyAsync(Permissions.Programs_View, MatchTypes.Controller,
                controller: "Programs", priority: 100, notes: "Programs overview");

            // Intake
            await UpsertEndpointPolicyAsync(Permissions.Intake_View, MatchTypes.Controller, controller: "Intake", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.Intake_Create, MatchTypes.ControllerAction, controller: "Intake", action: "Create", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Intake_Edit, MatchTypes.ControllerAction, controller: "Intake", action: "Edit", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Intake_Assign, MatchTypes.ControllerAction, controller: "Intake", action: "Assign", priority: 50);

            // Clients
            await UpsertEndpointPolicyAsync(Permissions.Clients_View, MatchTypes.Controller, controller: "Clients", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.Clients_Create, MatchTypes.ControllerAction, controller: "Clients", action: "Create", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Clients_Edit, MatchTypes.ControllerAction, controller: "Clients", action: "Edit", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Clients_Archive, MatchTypes.ControllerAction, controller: "Clients", action: "Archive", priority: 50);

            // Applications
            await UpsertEndpointPolicyAsync(Permissions.Applications_View, MatchTypes.Controller, controller: "Applications", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.Applications_Edit, MatchTypes.ControllerAction, controller: "Applications", action: "Edit", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Applications_Assign, MatchTypes.ControllerAction, controller: "Applications", action: "Assign", priority: 50);

            // Assessment & Approvals
            await UpsertEndpointPolicyAsync(Permissions.Assessment_View, MatchTypes.Controller, controller: "Assessment", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.Approvals_L1, MatchTypes.ControllerAction, controller: "Approvals", action: "Level1", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Approvals_L2, MatchTypes.ControllerAction, controller: "Approvals", action: "Level2", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Approvals_L3, MatchTypes.ControllerAction, controller: "Approvals", action: "Level3", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Approvals_L4, MatchTypes.ControllerAction, controller: "Approvals", action: "Level4", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Approvals_L5, MatchTypes.ControllerAction, controller: "Approvals", action: "Level5", priority: 50);

            // Payments
            await UpsertEndpointPolicyAsync(Permissions.Payments_View, MatchTypes.Controller, controller: "Payments", priority: 100);
            await UpsertEndpointPolicyAsync(Permissions.Payments_Validate, MatchTypes.ControllerAction, controller: "Payments", action: "Validate", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Payments_ExportLists, MatchTypes.ControllerAction, controller: "Payments", action: "ExportLists", priority: 50);
            await UpsertEndpointPolicyAsync(Permissions.Payments_Reconcile, MatchTypes.ControllerAction, controller: "Payments", action: "Reconcile", priority: 50);

            // Form design/admin vs builder vs submit
            // Form Management (CRUD dashboard & related tables)
            await UpsertEndpointPolicyAsync(
                Permissions.Forms_Manage,
                MatchTypes.Controller,
                controller: "Form",
                priority: 100,
                notes: "Form Management dashboard");

            // Form Builder UI (your Create action on the same controller)
            await UpsertEndpointPolicyAsync(
                Permissions.Forms_Builder,
                MatchTypes.ControllerAction,
                controller: "Form",
                action: "Create",
                priority: 50,
                notes: "Form Builder action");

            // Submit (if you have a dedicated Submit action on FormController)
            await UpsertEndpointPolicyAsync(
                Permissions.Forms_Submit,
                MatchTypes.ControllerAction,
                controller: "Form",
                action: "Submit",
                priority: 50,
                notes: "Submit entries");




        }
    }
}
