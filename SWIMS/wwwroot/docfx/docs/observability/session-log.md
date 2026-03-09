# Session Log

The Session Log records authentication events — sign-ins and sign-outs — for every user. It provides visibility into user activity patterns and aids in security investigations.

## How Sessions Are Tracked

`SessionCookieEvents` (`Services/Diagnostics/Sessions/`) hooks into the ASP.NET Core application cookie:

### On Sign-In (`ValidatePrincipal` / cookie sign-in event)

```csharp
// A new SessionLog row is inserted:
new SessionLog
{
    UserId   = userId,
    Username = username,
    LoginAt  = DateTimeOffset.UtcNow,
    IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
    UserAgent = httpContext.Request.Headers["User-Agent"],
    IsActive  = true
}
```

### On Sign-Out (cookie `SigningOut` event)

The most recent active session for the user is updated:

```csharp
session.LogoutAt = DateTimeOffset.UtcNow;
session.IsActive = false;
```

### Unclean Exits

If a user closes the browser without explicitly logging out, `LogoutAt` remains `null` and `IsActive = true`. The session will stay "active" in the log indefinitely. This is expected behaviour — there is no session timeout mechanism in v1 that would auto-close these entries.

> [!TIP]
> A future enhancement could add a Hangfire job that marks sessions older than N hours as inactive if no activity has been recorded.

## Viewing Session Logs

**Route**: `GET /Portal/Logs/Sessions`  
**Permission**: `Admin.SessionLogs`

The session log view shows:

| Column | Description |
|--------|-------------|
| Username | The user who logged in |
| Login At | UTC timestamp of login |
| Logout At | UTC timestamp of logout (blank = still active or unclean exit) |
| IP Address | Client IP at time of login |
| User Agent | Browser/client identifier |
| Active | Green badge if session is still active |

### Filters

- Username search
- Active-only toggle
- Date range

## Security Notes

- Session logs are visible only to users with `Admin.SessionLogs` permission.
- IP addresses are logged as-is from `RemoteIpAddress`. If SWIMS is behind a reverse proxy, ensure `ForwardedHeaders` middleware is configured so the client IP is correctly extracted from `X-Forwarded-For`.
- Session log rows are not deleted when a user is deleted — they are retained for audit continuity.
