---
_layout: landing
---

# SWIMS Developer Documentation

**Social Welfare Information Management System** — Government of the Commonwealth of Dominica, in partnership with the World Food Programme.

SWIMS is an ASP.NET Core 9 MVC + Razor Pages web application that provides end-to-end social welfare case management: beneficiary registration, programme-linked form submissions, a five-level approval workflow, benefit period tracking, reporting, and more.

> [!NOTE]
> This documentation is generated from source using **DocFX v2**. It covers the developer architecture, module design, configuration, and integration points. For the end-user guide, refer to the separate SWIMS User Guide document.

## Quick Links

| Area | Description |
|------|-------------|
| [Getting Started](docs/getting-started.md) | Dev environment setup, running the app |
| [Architecture](docs/architecture.md) | Project layout, tech stack, DB contexts |
| [Authentication](docs/auth/overview.md) | Identity, LDAP, 2FA |
| [Authorization](docs/authorization/overview.md) | RBAC, dynamic policies, endpoint policies |
| [Programmes & Forms](docs/programmes-forms/overview.md) | Form builder, approvals workflow |
| [Cases](docs/cases/overview.md) | Case lifecycle, benefit period, background sweeps |
| [Notifications](docs/notifications/overview.md) | Bell UI, email, digest, web push |
| [Messaging](docs/messaging/overview.md) | Real-time chat via SignalR |
| [Email Service](docs/email/overview.md) | SMTP profiles, Microsoft Graph transport |
| [Reporting](docs/reporting/overview.md) | SSRS integration, reverse proxy |
| [Observability](docs/observability/overview.md) | Audit log, session log, Serilog |
| [API Module](docs/api/overview.md) | Versioned REST API `/api/v1`, API Dashboard |
| [Background Jobs](docs/background-jobs/overview.md) | Hangfire, outbox, digest, case sweeps |
| [Elsa Workflows](docs/elsa-workflows/overview.md) | External Elsa v3 integration |

## Current Version

**SWIMS v1.6.0.0** — ASP.NET Core 9 · SQL Server · DocFX 2.78.3
