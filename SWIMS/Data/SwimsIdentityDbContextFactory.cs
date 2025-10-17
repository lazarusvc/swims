using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SWIMS.Data;

public sealed class SwimsIdentityDbContextFactory
    : IDesignTimeDbContextFactory<SwimsIdentityDbContext>
{
    public SwimsIdentityDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Pick the correct connection string key used by your identity db
        var cs =
            cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No connection string found for SwimsIdentityDbContext.");

        var opts = new DbContextOptionsBuilder<SwimsIdentityDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new SwimsIdentityDbContext(opts);
    }
}
