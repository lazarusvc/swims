# Notification Routing

*Requires `Admin.NotificationsRouting` permission.*

Navigate to **Admin → Notification Routing** to configure the default notification delivery channels for each event type system-wide.

## What Is Notification Routing?

By default, SWIMS has built-in channel defaults for each notification type (e.g. a new message triggers an in-app notification and an email). Notification Routing lets you override these defaults globally — for example, disabling email for a low-priority event type, or enabling push for all events.

Individual users can further personalise their preferences from their own [Notification Preferences](../notifications/preferences.md) page. The routing rules set here are the **system defaults** that apply when a user has not made their own choice.

## Routing Rules List

The list shows each notification type with its current routing configuration:

| Column | Description |
|--------|-------------|
| **Type** | The notification event name (e.g. `NewMessage`) |
| **In-App** | Whether the bell notification is enabled by default |
| **Email** | Whether an immediate email is sent by default |
| **Digest** | Whether the event is included in the daily digest by default |
| **Push** | Whether a web push notification is sent by default |

## Editing a Routing Rule

1. Click **Edit** next to a notification type.
2. Toggle the channels on or off.
3. Click **Save**.

Changes take effect immediately for all users who have not overridden that type in their personal preferences.

## Adding a New Rule

If a new notification type has been introduced by a developer but has no routing rule yet, it will use the system default (in-app only). To set a specific rule:

1. Click **New Routing Rule**.
2. Enter the **Type** name exactly as it appears in the notification data.
3. Configure the channels.
4. Click **Save**.
