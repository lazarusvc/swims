# Notifications — Overview

The Notifications module provides multi-channel event notification: in-app bell, real-time SignalR updates, per-user preferences, email, daily digests, and web push.

## End-to-End Flow

```
Producer (e.g. Approvals, Messaging)
  │
  ▼
INotifier.NotifyUserAsync(userId, username, type, payload)
  │
  ├──► DB: notify.notifications  (always)
  │
  ├──► SignalR: NotifsHub → group "u:{userId}"  (immediate)
  │
  ├──► Web Push: WebPushSender → active subscriptions  (best-effort, swallows errors)
  │
  └──► Email Outbox (if user prefs allow email for this type)
         └── NotificationEmailComposer → IEmailOutbox.EnqueueAsync
               └── Hangfire: EmailOutboxJobs (minutely)

Daily at 08:00
  └── NotificationDigestJobs → aggregate unsent digest items → single email per user
```

## Key Components

| Component | File | Role |
|-----------|------|------|
| `INotifier` / `Notifier` | `Services/Notifications/Notifier.cs` | Orchestrates all notification channels |
| `INotificationPreferences` | `Services/Notifications/NotificationPreferences.cs` | Resolves effective per-user, per-type flags |
| `INotificationEmailComposer` | `Services/Notifications/NotificationEmailComposer.cs` | Builds email subject/body from payload |
| `IWebPushSender` / `WebPushSender` | `Services/Notifications/WebPushSender.cs` | Sends VAPID push to active subscriptions |
| `INotificationDispatcher` | `Services/Notifications/NotificationDispatcher.cs` | Hangfire-queued delivery orchestration |
| `NotifsHub` | `Web/Hubs/NotifsHub.cs` | SignalR hub for real-time bell updates |
| `SwimsEventKeys` | `Services/Notifications/SwimsEventKeys.cs` | Constants for notification type strings |

## Notification Types

Standard event types (defined in `SwimsEventKeys`):

| Key | Event |
|-----|-------|
| `NewMessage` | A new chat message was received |
| *(custom)* | Any string — producers define their own type keys |

## `notify.notifications` Schema

```sql
CREATE TABLE notify.notifications (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId      INT NOT NULL,
    Username    NVARCHAR(256),
    Type        VARCHAR(64) NOT NULL,
    PayloadJson NVARCHAR(MAX),
    Seen        BIT NOT NULL DEFAULT 0,
    CreatedUtc  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

Indexes: `(UserId, CreatedUtc DESC)`, `(UserId, Seen, CreatedUtc DESC)`, `(Type)`.

## REST API Endpoints

| Route | Purpose |
|-------|---------|
| `GET /me/notifications?unseenOnly=&skip=&take=` | Paginated notification inbox |
| `GET /me/notifications/count` | Unseen notification count (badge) |
| `POST /me/notifications/{id}/seen` | Mark single notification as seen |
| `POST /me/notifications/seen` | Mark all as seen |
| `GET /me/notifications/types` | Distinct recent notification types (for prefs UI) |
| `GET /me/notifications/prefs/effective?type=TypeName` | Effective flags for a type |
| `PUT /me/notifications/prefs` | Upsert user pref override for a type |

## Bell UI

`Views/Shared/WowDash/Partials/_NotifBell.cshtml` + `wwwroot/js/notifs.js`:

- Connects to `/hubs/notifs` via SignalR on page load.
- On `notif` hub event: prepends new notification to dropdown list, increments unseen badge.
- Loads existing notifications via `GET /me/notifications` on dropdown open.
- Marks seen on click.

## Notification Routing (Admin)

Admins can configure **per-type routing rules** via **Admin → Notification Routing** (`Admin.NotificationsRouting` permission). This allows overriding default email/push/in-app channel behaviour for specific event types without code changes.

## Related Pages

- [Web Push (PWA)](web-push.md)
