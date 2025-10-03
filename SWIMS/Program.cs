// -------------------------------------------------------------------
// File:    Program.cs
// Author:  N/A
// Created: N/A
// Purpose: Entry point for SWIMS ASP.NET Core application; configures services, middleware, and runs the web host.
// Dependencies:
//   - Microsoft.AspNetCore.Builder, Hosting, Identity, EF Core, Configuration
//   - SWIMS.Data.SwimsIdentityDbContext, SWIMS.Models.SwUser, SWIMS.Models.SwRole
//   - SWIMS.Services.BcryptPasswordHasher, LdapAuthService, SeedData
// -------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SWIMS.Data;
using SWIMS.Data.Reports;
using SWIMS.Models;
using SWIMS.Models.StoredProcs;
using SWIMS.Services;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;
using SWIMS.Services.Email;
using SWIMS.Services.Reporting;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Configure database context and EF Core migrations
// ------------------------------------------------------
builder.Services.AddDbContext<SwimsIdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Identity", "auth")
    ));

builder.Services.AddDbContext<SwimsDb_moreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_More", "dbo")
    ));

builder.Services.AddDbContext<SwimsStoredProcsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "sp")
    ));

builder.Services.Configure<StoredProcOptions>(
    builder.Configuration.GetSection("StoredProcs"));

builder.Services.AddDbContext<SwimsReportsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Reports", "rpt")
    ));


builder.Services.Configure<ReportingOptions>(builder.Configuration.GetSection("Reporting"));
builder.Services.AddScoped<ISsrsUrlBuilder, SsrsUrlBuilder>();

// Configure Razor Pages
builder.Services.AddRazorPages();

// ------------------------------------------------------
// Configure Identity and authentication services
// ------------------------------------------------------
builder.Services
    .AddDefaultIdentity<SwUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    })
    .AddRoles<SwRole>()
    .AddEntityFrameworkStores<SwimsIdentityDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("ProgramManager", p => p.RequireRole("Admin", "ProgramManager"));
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPolicyStore, EfPolicyStore>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DbAuthorizationPolicyProvider>();

builder.Services.AddSingleton<IEndpointCatalog, EndpointCatalog>();


// Use BCrypt for password hashing
builder.Services.AddScoped<IPasswordHasher<SwUser>, BcryptPasswordHasher>();

// LDAP authentication service singleton
builder.Services.AddSingleton<ILdapAuthService, LdapAuthService>();

// Stored Procedures Module
builder.Services.AddDataProtection(); // optional but recommended if using per-proc SQL logins
builder.Services.AddSingleton<StoredProcedureRunner>();


// Configure application cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


// Add services to the container.
// ------------------------------------------------------
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPublicAccessStore, EfPublicAccessStore>();
builder.Services.AddScoped<IEndpointPolicyAssignmentStore, EfEndpointPolicyAssignmentStore>();
builder.Services.AddScoped<IAuthorizationHandler, PublicOrAuthenticatedHandler>();

// enable fallback (allow public if in DB, else require auth)
var enablePublicFallback = builder.Configuration.GetValue<bool?>("Auth:EnablePublicOrAuthenticatedFallback") ?? true;
builder.Services.AddAuthorization(options =>
{
    if (enablePublicFallback)
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PublicOrAuthenticatedRequirement())
            .Build();
    }

    // keep parachute static policies
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("ProgramManager", p => p.RequireRole("Admin", "ProgramManager"));
});

// Add global filter to enforce DB endpoint→policy assignments
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<SWIMS.Services.Auth.DbEndpointPolicyFilter>();
});

// Global authorization policy: require authenticated users for all MVC controllers
//builder.Services.AddControllersWithViews(options =>
//{
//    var policy = new AuthorizationPolicyBuilder()
//                     .RequireAuthenticatedUser()
//                     .Build();
//    options.Filters.Add(new AuthorizeFilter(policy));


builder.Services.AddHttpClient("ssrs-proxy", c =>
{
    c.Timeout = TimeSpan.FromSeconds(180); // tolerate slow first renders
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseDefaultCredentials = true,
    PreAuthenticate = true,
    AllowAutoRedirect = false,
    UseCookies = true,                              // <— important: SSRS may set cookies
    CookieContainer = new CookieContainer(),
    UseProxy = false
});

// Emailing (SMTP + templates)
builder.Services.AddSwimsEmailing(builder.Configuration);

// ASP.NET Identity email adapter
builder.Services.AddTransient<IEmailSender, IdentityEmailSender>();
builder.Services.AddTransient<IEmailSender<SwUser>, IdentityEmailSenderAdapter>();

// Register the one-time startup test in Development only
// builder.Services.AddHostedService<SWIMS.Services.Email.StartupEmailSmokeTest>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Run pending migrations (Identity DB)
        var db_1 = services.GetRequiredService<SwimsIdentityDbContext>();
        db_1.Database.Migrate();
        //  re-enable for second DB:
        // var db_2 = services.GetRequiredService<SwimsDb_moreContext>();
        // db_2.Database.Migrate();
        
        // Seed roles  admin + policies
        await SeedData.EnsureSeedDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seeding failed at startup.");
        throw; // fail fast so we see the real error
    }
}



    // ------------------------------------------------------
    // Configure HTTP request pipeline
    // ------------------------------------------------------
    if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for
    // production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Ensure default wwwroot static files are served
app.UseStaticFiles();

// Serve generated DocFX documentation at /docs
app.UseFileServer(new FileServerOptions
{
    RequestPath = "/docs",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "docs")
    ),
    EnableDefaultFiles = true,
    EnableDirectoryBrowsing = false
});


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();
app.Run();
