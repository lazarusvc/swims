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
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.StoredProcs;
using SWIMS.Services;

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
        options.SignIn.RequireConfirmedAccount = false;
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    })
    .AddRoles<SwRole>()
    .AddEntityFrameworkStores<SwimsIdentityDbContext>()
    .AddDefaultTokenProviders();

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

// Global authorization policy: require authenticated users for all MVC controllers
    //builder.Services.AddControllersWithViews(options =>
    //{
    //    var policy = new AuthorizationPolicyBuilder()
    //                     .RequireAuthenticatedUser()
    //                     .Build();
    //    options.Filters.Add(new AuthorizeFilter(policy));
    //});


var app = builder.Build();


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

// Run pending migrations and seed default data
var db_1 = services.GetRequiredService<SwimsIdentityDbContext>();
// var db_2 = services.GetRequiredService<SwimsDb_moreContext>();
db_1.Database.Migrate();
// db_2.Database.Migrate();
await SeedData.EnsureSeedDataAsync(services);



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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();
app.Run();
