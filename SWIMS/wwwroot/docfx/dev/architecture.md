# Architecture

## Project Layout

```
SWIMS/
├── Areas/
│   ├── Admin/              # Admin area: API dashboard, authorization, reports admin, system settings
│   │   ├── Controllers/    # AdminArea MVC controllers
│   │   ├── ViewModels/     # Admin-specific view models
│   │   └── Views/          # Razor views
│   ├── Identity/           # Scaffolded ASP.NET Identity Razor Pages
│   │   └── Pages/Account/  # Login, Register, 2FA, Manage
│   └── Portal/             # Authenticated user portal pages
│       └── Pages/          # Logs, Messenger, Notifications prefs
├── Controllers/            # Main MVC controllers (Forms, Cases, Approvals, Beneficiaries, etc.)
│   ├── Dev/                # Development/testing controllers (excluded from prod)
│   └── Integration/        # Elsa callback integration controller
├── Data/                   # EF Core DbContext classes (see below)
│   ├── Cases/
│   ├── Lookups/
│   └── Reports/
├── Helpers/                # Identity/claims extension helpers
├── Migrations/             # EF Core migrations (one folder per context)
├── Models/                 # Entity and view models
│   ├── Email/
│   ├── Logging/
│   ├── Lookups/
│   ├── Messaging/
│   ├── Notifications/
│   ├── Outbox/
│   ├── Reports/
│   ├── Security/
│   ├── StoredProcs/
│   └── ViewModels/
├── Options/                # Strongly-typed options classes
├── Security/               # Permissions constants + Elsa key filter
├── Services/               # All business logic services
│   ├── Auth/               # Policy provider, endpoint filter, stores
│   ├── Cases/              # Case lifecycle + background sweeps
│   ├── Diagnostics/        # Auditing and session logging
│   ├── Elsa/               # Elsa v3 HTTP client & queue
│   ├── Email/              # Email service (SMTP / Graph)
│   ├── Messaging/          # Chat presence
│   ├── Notifications/      # Notifier, dispatcher, push, preferences
│   ├── Outbox/             # Email outbox + Hangfire job
│   ├── Reporting/          # SSRS URL builder
│   ├── Setup/              # First-run setup state service
│   └── SystemSettings/     # Dynamic system settings
├── Templates/
│   ├── Email/              # Handlebar HTML email templates
│   └── ViewGenerator/      # Code scaffolding templates
├── Views/                  # MVC Razor views (all main modules)
├── Web/
│   ├── Endpoints/          # Minimal API endpoints (/api/v1)
│   ├── Hubs/               # SignalR hubs (ChatsHub, NotifsHub)
│   ├── Ops/                # EndpointCatalog
│   └── Setup/              # Wizard setup Razor component
├── Program.cs              # App entry point & DI composition root
├── SWIMS.csproj            # Project file (DocFX build integration)
└── wwwroot/
    ├── docfx/              # DocFX source (this documentation)
    └── docs/               # DocFX output (generated static site)
```

## EF Core DbContexts

SWIMS uses **six separate DbContexts** sharing a single SQL Server database but isolated by schema. Each has its own EF migrations history table.

| Context | Schema | History Table | Purpose |
|---------|--------|--------------|---------|
| `SwimsIdentityDbContext` | `auth`, `notify`, `msg`, `ops`, `ac` | `__EFMigrationsHistory_Identity` | Identity users/roles, notifications, messaging, audit/session log, access control |
| `SwimsDb_moreContext` | `dbo` | `__EFMigrationsHistory_More` | Core forms, beneficiaries, cases (legacy) |
| `SwimsCasesDbContext` | `case` | `__EFMigrationsHistory_Cases` | Cases module (SW_case, SW_caseForm, SW_caseAssignment) |
| `SwimsLookupDbContext` | `ref` | `__EFMigrationsHistory_Lookup` | Reference data: program tags, form types, lookup links |
| `SwimsStoredProcsDbContext` | `sp` | `__EFMigrationsHistory` | Stored procedure registry + params |
| `SwimsReportDbContext` | `rpt` | `__EFMigrationsHistory_Reports` | SSRS report definitions + params |

> [!IMPORTANT]
> All contexts use the same `DefaultConnection` string. Never run `dotnet ef database update` without specifying `--context`, as EF may target the wrong context.

## Request Pipeline

```
Browser
  │
  ▼
ForwardedHeaders middleware  (reverse-proxy X-Forwarded-For/Proto)
X-Forwarded-Prefix middleware (PathBase from proxy header)
  │
  ▼
UsePathBase  (App:PathBase in Production)
  │
  ▼
Serilog request logging
  │
  ▼
HTTPS Redirection → Static Files
  │
  ▼
Routing
  │
  ▼
Authentication (Identity cookie)
  │
  ▼
Authorization
  ├── DbAuthorizationPolicyProvider  (loads policies from DB)
  ├── DbEndpointPolicyFilter (MVC action filter: endpoint → policy assignment)
  └── PublicOrAuthenticatedRequirement (fallback policy)
  │
  ▼
SignalR hubs (/hubs/chats, /hubs/notifs)
  │
  ▼
Hangfire Dashboard (/hangfire — Admin.Hangfire permission)
  │
  ▼
Minimal API (/api/v1/*)
  │
  ▼
Razor Pages (Identity, Portal)
  │
  ▼
MVC Controllers (all main modules)
```

## Hangfire Queue Design

Three separate Hangfire servers handle distinct workloads:

| Server | Queue | Workers | Jobs |
|--------|-------|---------|------|
| Notifications server | `notifications` | 10 | Elsa workflow dispatch, notification delivery |
| Outbox server | `outbox` | 1 | Email outbox dispatch (minutely) |
| Default server | `default` | 1 | Any unqueued/miscellaneous jobs |

Recurring jobs registered on startup (when `Hangfire:ScheduleOnStartup = true`):
- `email-outbox-dispatch` — every minute
- `notification-digest-daily` — daily at 08:00 server time

## SignalR Hubs

| Hub | Route | Purpose |
|-----|-------|---------|
| `ChatsHub` | `/hubs/chats` | Real-time messaging (send/receive, typing indicators) |
| `NotifsHub` | `/hubs/notifs` | Real-time notification bell updates |
