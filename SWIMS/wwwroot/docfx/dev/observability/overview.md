# Observability — Overview

SWIMS provides three layers of observability: **structured request logging** via Serilog, an **Audit Log** for entity-level change tracking, and a **Session Log** for user authentication events.

## Serilog (Structured Logging)

Serilog is configured in `Program.cs` with two sinks by default:

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .CreateLogger();
builder.Host.UseSerilog();
```

### Configuration (appsettings)

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "System": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/swims-.log",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30
      }
    }
  ],
  "Enrich": ["FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId"]
}
```

### Request Logging Enrichment

`UseSerilogRequestLogging` enriches each HTTP request log with:
- `UserId` — authenticated user name or `"anonymous"`
- `ClientIP` — remote IP address
- `RequestPath` — request path

## Audit Log

The Audit Log captures **entity-level changes** (Create, Update, Delete) automatically via `AuditSaveChangesInterceptor`.

### How It Works

`AuditSaveChangesInterceptor` is registered on `SwimsIdentityDbContext`:

```csharp
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
// Registered in DbContext configuration:
options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
```

On every `SaveChangesAsync`:
1. The interceptor scans `ChangeTracker` for `Added`, `Modified`, and `Deleted` entries on audited entity types.
2. For `Modified` entries: captures `OldValues` (original) and `NewValues` (current) as JSON diffs.
3. Writes `AuditLog` rows with the current user's ID/name, action type, entity name, entity ID, and value snapshots.
4. The audit write happens in the same transaction as the data change.

### `AuditLog` Model

```csharp
public class AuditLog
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; }       // "Created", "Modified", "Deleted"
    public string EntityName { get; set; }   // Class name of the entity
    public string? EntityId { get; set; }    // PK value as string
    public string? OldValues { get; set; }   // JSON
    public string? NewValues { get; set; }   // JSON
}
```

### Viewing Audit Logs

**Route**: `GET /Portal/Logs/Audit` (requires `Admin.AuditLogs`)

Filters:
- Username / User ID
- Action type (Created / Modified / Deleted)
- Entity name
- Date range

Supports **CSV export** of filtered results.

## Session Log

The Session Log records user sign-in and sign-out events.

### How It Works

`SessionCookieEvents` is hooked into the application cookie:

```csharp
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.EventsType = typeof(SessionCookieEvents);
});
```

- **On `SigningIn`**: inserts a `SessionLog` row with `LoginAt`, IP address, user agent, and `IsActive = true`.
- **On `SigningOut`**: stamps `LogoutAt` and sets `IsActive = false` on the open session row.

### `SessionLog` Model

```csharp
public class SessionLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public DateTimeOffset LoginAt { get; set; }
    public DateTimeOffset? LogoutAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; }
}
```

### Viewing Session Logs

**Route**: `GET /Portal/Logs/Sessions` (requires `Admin.SessionLogs`)

Shows active and historical sessions. Useful for security audits and identifying concurrent or anomalous logins.

## Related Pages

- [Audit Log Details](audit-log.md)
- [Session Log Details](session-log.md)
