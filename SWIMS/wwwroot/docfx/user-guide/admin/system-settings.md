# System Settings

*Requires `Admin.Settings` permission.*

Navigate to **Admin → System Settings** to configure global application settings. Changes take effect immediately — no restart required.

## Site Identity

| Setting | Description |
|---------|-------------|
| **Site Name** | The application name shown in the header and browser tab (e.g. "SWIMS") |
| **Site Tagline** | A short descriptive subtitle shown in the header |
| **Logo URL** | Path or URL to the site logo image |
| **Organisation Name** | The name of the ministry or organisation (e.g. "Ministry of Social Services") |

## Support Contacts

These values appear in system emails sent to users (password resets, notifications, etc.).

| Setting | Description |
|---------|-------------|
| **Support Email** | The helpdesk or IT support email address |
| **Support Phone** | The helpdesk or IT support phone number |

## Features

| Setting | Description |
|---------|-------------|
| **Enable Registration** | Allow users to self-register accounts. Disable this in production if all accounts are created by admins or via AD. |
| **Maintenance Mode** | When enabled, shows a maintenance banner to all users. Useful during updates or scheduled downtime. |
| **Maintenance Message** | The message shown when Maintenance Mode is active. |

## Notification Defaults

| Setting | Description |
|---------|-------------|
| **Notification From Name** | The "From" display name on system emails (e.g. "SWIMS Notifications") |
| **Notification From Email** | The sending email address for system emails |

> [!NOTE]
> The Notification From Email must match the email account configured in the email service settings (`appsettings.json`). Changing it here alone will not change the actual sending account — contact your system administrator for email transport configuration.
