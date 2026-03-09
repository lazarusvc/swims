# Email Service — Overview

SWIMS v2 email service supports two transport modes: **SMTP Profiles** and **Microsoft Graph**. The mode is selected at configuration time via `Emailing:Mode`.

## Architecture

```
IEmailService
    ├── SmtpEmailService      (Mode = "Smtp")
    └── GraphEmailService     (Mode = "Graph")

IEmailSender (Identity)
    └── IdentityEmailSender   → delegates to IEmailService

IEmailSender<SwUser> (Identity UI)
    └── IdentityEmailSenderAdapter → delegates to IdentityEmailSender
```

`AddSwimsEmailing(config)` (in `EmailingServiceCollectionExtensions`) registers the correct implementation based on `Emailing:Mode`.

## SMTP Profiles

Multiple SMTP profiles are supported, with one active at a time. Profiles are defined under `Emailing:SmtpProfiles:Profiles` and selected by `Emailing:SmtpProfiles:ActiveProfile`.

### Built-in Profiles

| Profile | Host | Notes |
|---------|------|-------|
| `Gmail` | `smtp.gmail.com:587` | Requires App Password (not regular password) |
| `Microsoft365` | `smtp.office365.com:587` | Service account or app password |
| `LocalPickup` | null | Dev pickup directory; emails saved as `.eml` files |

### Gmail App Password Setup

1. Enable 2-Step Verification on the Gmail account.
2. Go to Google Account → Security → App passwords → Generate for "Mail" / "Other".
3. Copy the 16-character password.
4. Set in `Emailing:SmtpProfiles:Profiles:Gmail:Password`.
5. Ensure `Host = smtp.gmail.com`, `Port = 587`, `UseStartTls = true`, `UseSsl = false`.

### LocalPickup (Dev)

Set `DevPickupDirectory` to a local path (e.g., `.smtp-outbox`). SWIMS writes `.eml` files there instead of sending. Use any `.eml` viewer to inspect sent emails during development.

## Microsoft Graph Transport

Graph is Microsoft's recommended approach for server apps — avoids SMTP basic auth and Security Defaults friction.

### Setup

1. Register an app in **Entra ID** (Azure AD).
2. Grant **Application** permission: `Mail.Send`.
3. Grant admin consent.
4. Optionally scope via **Exchange Online RBAC for Applications** (`New-ManagementRoleAssignment`) to restrict which mailboxes the app can send from.

### Configuration

```json
"Emailing": {
  "Mode": "Graph",
  "Graph": {
    "TenantId": "00000000-0000-0000-0000-000000000000",
    "ClientId": "00000000-0000-0000-0000-000000000000",
    "ClientSecret": "<use user-secrets>",
    "SenderUser": "service@yourdomain.com",
    "SaveToSentItems": true,
    "DefaultFromAddress": "service@yourdomain.com",
    "DefaultFromName": "SWIMS"
  }
}
```

> [!WARNING]
> Store `ClientSecret` in `dotnet user-secrets` (dev) or Azure Key Vault / environment variables (production). Never commit it to source control.

## Startup Smoke Test

`StartupEmailSmokeTest` is a hosted service that can send a test email on startup:

```json
"Emailing": {
  "StartupTest": {
    "Enabled": true,
    "To": "developer@example.com",
    "TemplateKey": "Startup",
    "Subject": "SWIMS started",
    "DelaySeconds": 5
  }
}
```

Set `Enabled: false` in production or CI environments.

## Email Outbox

Production email sending is **async via the Email Outbox**. Rather than sending directly, services call `IEmailOutbox.EnqueueAsync(...)` which persists a row in `notify.email_outbox`. Hangfire's `EmailOutboxJobs` dequeues and sends emails **every minute** via the configured transport.

This prevents blocking web requests on slow SMTP calls and provides automatic retry on failure.

See [Background Jobs — Overview](../background-jobs/overview.md) for outbox job details.

## Related Pages

- [Email Templates](templates.md)
