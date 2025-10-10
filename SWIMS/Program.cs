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

using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using SWIMS.Data;
using SWIMS.Data.Reports;
using SWIMS.Models;
using SWIMS.Models.StoredProcs;
using SWIMS.Services;
using SWIMS.Services.Auth;
using SWIMS.Services.Diagnostics;
using SWIMS.Services.Diagnostics.Auditing;
using SWIMS.Services.Diagnostics.Sessions;
using SWIMS.Services.Email;
using SWIMS.Services.Notifications;
using SWIMS.Services.Outbox;
using SWIMS.Services.Outbox.Jobs;
using SWIMS.Services.Reporting;
using SWIMS.Web.Endpoints;
using SWIMS.Web.Hubs;
using SWIMS.Web.Ops;
using System.Net;
using System.Threading;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Serilog bootstrap (read from config + dev-friendly console)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

// ------------------------------------------------------
// Configure database context and EF Core migrations
// ------------------------------------------------------
builder.Services.AddDbContext<SwimsIdentityDbContext>((sp, options) =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Identity", "auth")
    );
    options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
});

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

// DI
builder.Services.AddScoped<ISessionLogger, SessionLogger>();
builder.Services.AddScoped<SessionCookieEvents>();

// Identity cookie events hookup (after AddIdentity / before app.Build())
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.EventsType = typeof(SessionCookieEvents);
    // keep your existing cookie settings if any
});

builder.Services.AddSignalR();
builder.Services.AddScoped<INotifier, Notifier>();

builder.Services.AddScoped<IEmailOutbox, EmailOutboxService>();
builder.Services.AddScoped<EmailOutboxJobs>();

builder.Services.AddScoped<INotificationEmailComposer, NotificationEmailComposer>();


builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
       {
           SchemaName = "ops",
           PrepareSchemaIfNecessary = true,
           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
           QueuePollInterval = TimeSpan.FromSeconds(15),
           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
           UseRecommendedIsolationLevel = true
       });
});

// Hangfire Server
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "outbox", "default" }; // "outbox" first = higher priority
});


builder.Services.Configure<ReportingOptions>(builder.Configuration.GetSection("Reporting"));
builder.Services.AddScoped<ISsrsUrlBuilder, SsrsUrlBuilder>();

// Configure Razor Pages
builder.Services.AddRazorPages(options =>
{
    // Require auth for all Portal pages by default
    options.Conventions.AuthorizeAreaFolder("Portal", "/");
});

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
builder.Services.AddScoped<IPasswordHasher<SwUser>, CompatibleBcryptHasher>();

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

// Health endpoints (lightweight)
builder.Services.AddHealthChecks();

builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseConsole() // 👈 add this
       .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
       {
           SchemaName = "ops",
           PrepareSchemaIfNecessary = true,
           SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
           QueuePollInterval = TimeSpan.FromSeconds(15),
           CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
           UseRecommendedIsolationLevel = true
       });
});

builder.Services.AddScoped<INotificationPreferences, NotificationPreferences>();



var app = builder.Build();


//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        // Run pending migrations (Identity DB)
//        var db_1 = services.GetRequiredService<SwimsIdentityDbContext>();
//        db_1.Database.Migrate();
//        //  re-enable for second DB:
//        // var db_2 = services.GetRequiredService<SwimsDb_moreContext>();
//        // db_2.Database.Migrate();
        
//        // Seed roles  admin + policies
//        await SeedData.EnsureSeedDataAsync(services);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "Seeding failed at startup.");
//        throw; // fail fast so we see the real error
//    }
//}



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

app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("UserId", http.User?.Identity?.IsAuthenticated == true ? http.User.Identity!.Name : "anonymous");
        diag.Set("ClientIP", http.Connection.RemoteIpAddress?.ToString());
        diag.Set("RequestPath", http.Request.Path);
    };
});

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

app.Use(async (ctx, next) =>
{
    // Basic self-redirect guard for login routes
    var p = ctx.Request.Path.Value?.ToLowerInvariant() ?? "";
    if (p.StartsWith("/account/login") || p == "/healthz" || p == "/readyz" || p.StartsWith("/_framework") || p.StartsWith("/css") || p.StartsWith("/js"))
    {
        // let these pass without auth challenge to avoid loops
    }
    await next();
});


app.UseAuthentication();
app.UseAuthorization();

app.MapSwimsCoreEndpoints();
app.MapHub<NotifsHub>("/hubs/notifs");

app.UseHangfireDashboard("/ops/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthFilter() },
    IsReadOnlyFunc = _ => false
});

// schedule the recurring dispatcher when app starts
using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurring.AddOrUpdate<EmailOutboxJobs>(
    "email-outbox-dispatch",
    j => j.RunOnceAsync(50, null, CancellationToken.None),
    Cron.Minutely);
}

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
