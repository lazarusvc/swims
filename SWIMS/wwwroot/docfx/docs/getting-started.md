# Getting Started

This guide walks you through setting up a local SWIMS development environment.

## Prerequisites

- **Visual Studio 2022** (v17.8+) or **Rider** with .NET 9 support
- **.NET 9 SDK**
- **SQL Server** (2019 or later) or SQL Server Express / LocalDB
- **Node.js** (for client-side asset tooling, if used)
- Optional: **Elsa v3 Server** instance (workflows are non-blocking — app runs without it)

## Clone & Restore

```bash
git clone <repo-url>
cd SWIMS
dotnet restore
```

## Configure `appsettings.Development.json`

Copy `appsettings.json` and create a `appsettings.Development.json` with overrides. At minimum you need:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SWIMS;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Emailing": {
    "Mode": "Smtp",
    "SmtpProfiles": {
      "ActiveProfile": "LocalPickup",
      "Profiles": {
        "LocalPickup": {
          "DefaultFromAddress": "no-reply@localhost",
          "DefaultFromName": "SWIMS (Dev)",
          "DevPickupDirectory": ".smtp-outbox"
        }
      }
    }
  },
  "Elsa": {
    "ServerUrl": "http://localhost:5001/",
    "ApiKey": "your-elsa-api-key",
    "Integration": {
      "NotificationsKey": "dev-key"
    }
  },
  "WebPush": {
    "Subject": "mailto:admin@example.com",
    "PublicKey": "<VAPID_PUBLIC_KEY>",
    "PrivateKey": "<VAPID_PRIVATE_KEY>"
  },
  "Hangfire": {
    "ScheduleOnStartup": true
  }
}
```

> [!TIP]
> Use `dotnet user-secrets` to keep sensitive values (Graph client secret, VAPID keys, LDAP bind password) out of source control.

## Run Database Migrations

SWIMS uses **six separate EF Core DbContexts**, each with its own schema and migration history table.

```bash
# Run from the SWIMS/ project folder

# Identity, Auth, Logging, Messaging, Notifications, AccessControl
dotnet ef database update --context SwimsIdentityDbContext

# Core forms, beneficiaries, cases (legacy DbMore)
dotnet ef database update --context SwimsDb_moreContext

# Cases module
dotnet ef database update --context SwimsCasesDbContext

# Lookup / Reference Data
dotnet ef database update --context SwimsLookupDbContext

# Stored Procedures registry
dotnet ef database update --context SwimsStoredProcsDbContext

# SSRS Report definitions
dotnet ef database update --context SwimsReportDbContext
```

> [!NOTE]
> On first run in Development, the app will also call `SeedData.EnsureSeedDataAsync()` which seeds default roles, permissions, and an admin user. Check `Data/SeedData.cs` for details.

## Run the Application

```bash
dotnet run
# or press F5 in Visual Studio
```

The app starts at `https://localhost:7XXX` (port from `launchSettings.json`).

## Build DocFX Documentation

The documentation system (this site) is embedded at `wwwroot/docfx/`. It is **disabled by default** to keep builds fast.

```bash
# One-time full build including docs
dotnet build -p:DocFxBuildEnabled=true

# Or manually from the docfx source folder
cd wwwroot/docfx
dotnet tool restore
dotnet tool run docfx metadata --logLevel verbose
dotnet tool run docfx build --logLevel verbose
```

The generated site is output to `wwwroot/docs/` and served at `/docs/` when the app runs.

## Key Configuration Sections

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string (all contexts share this) |
| `Emailing:Mode` | `"Smtp"` or `"Graph"` — selects email transport |
| `Elsa:ServerUrl` | Elsa v3 workflow server base URL |
| `Elsa:Integration:NotificationsKey` | Shared secret for Elsa → SWIMS callbacks |
| `WebPush:PublicKey` / `PrivateKey` | VAPID keys for web push notifications |
| `Auth:EnablePublicOrAuthenticatedFallback` | `true` = respect Public Endpoints table; `false` = require auth everywhere |
| `Auth:SeedOnStartup` | `true` = run seed in non-dev environments |
| `App:PathBase` | Path prefix for reverse proxy deployments (e.g. `/swims`) |
| `Hangfire:ScheduleOnStartup` | `true` = register recurring jobs on startup |
| `Reporting:SsrsBaseUrl` | Base URL of the SSRS report server |
