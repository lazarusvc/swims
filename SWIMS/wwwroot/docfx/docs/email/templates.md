# Email Templates

SWIMS uses **Handlebars.Net** for email template rendering, enabling HTML email templates with variable substitution, conditional blocks, and helpers.

## Template Files

Templates are stored in `Templates/Email/` as `.html` files with Handlebars syntax:

| File | Purpose |
|------|---------|
| `ConfirmEmail.html` | Email address confirmation for new accounts |
| `ConfirmEmailChange.html` | Confirmation when changing email address |
| `ResetPassword.html` | Password reset link |
| `TwoFactorCode.html` | TOTP code delivery (fallback when 2FA is set up) |
| `StartupProbe.html` | Startup smoke test email |
| `Notifications_Generic.html` | Generic notification (no contact footer) |
| `Notifications_GenericWithContacts.html` | Generic notification (with support contact footer) |
| `emailTemplate.master.withContacts.html` | Master layout with support contacts |
| `emailTemplate.master.noContacts.html` | Master layout without support contacts |
| `emailTemplate.inlineFallback.withContacts.html` | Fallback layout (inline CSS) |
| `emailTemplate.inlineFallback.noContacts.html` | Fallback layout (inline CSS, no contacts) |

## Template Keys

`TemplateKeys` (static class in `Services/Email/TemplateKeys.cs`) defines the keys used to reference templates:

```csharp
public static class TemplateKeys
{
    public const string ConfirmEmail         = "ConfirmEmail";
    public const string ConfirmEmailChange   = "ConfirmEmailChange";
    public const string ResetPassword        = "ResetPassword";
    public const string TwoFactorCode        = "TwoFactorCode";
    public const string Startup              = "Startup";
    public const string NotificationGeneric  = "Notifications_Generic";
    public const string NotificationWithContacts = "Notifications_GenericWithContacts";
}
```

## Template Variables (Common Tokens)

| Token | Description |
|-------|-------------|
| `SubjectLine` | Email subject |
| `BodyIntro` | Opening greeting |
| `MainParagraph` | Main body copy |
| `ShowCTA` | Boolean — show call-to-action button |
| `ActionUrl` | Button link URL |
| `ActionLabel` | Button text |
| `SupportEmail` | Support email address (from System Settings) |
| `SupportPhone` | Support phone number (from System Settings) |
| `ReferenceId` | Reference identifier (case number, notification ID, etc.) |

## Notification Email Tokens

`NotificationEmailComposer` supplies additional tokens for notification emails:

| Token | Description |
|-------|-------------|
| `TypeName` | Notification event type (e.g., `NewMessage`) |
| `PrefsUrl` | URL to notification preferences page |

### Subject Lines by Type

| Type | Subject |
|------|---------|
| `NewMessage` | New message from {Name} — SWIMS |
| *(default)* | New SWIMS notification |

## Adding a New Template

1. Create `Templates/Email/MyTemplate.html` with Handlebars syntax.
2. Add a constant to `TemplateKeys`.
3. Register in `EmailTemplateProvider` if auto-discovery is not used.
4. Call `IEmailService.SendTemplatedAsync(to, subject, TemplateKeys.MyTemplate, tokens)`.

## Absolute URLs in Background Jobs

Templates with CTAs require absolute URLs (including scheme and host). When composing emails from **background jobs** (e.g., Hangfire notification delivery), the `HttpContext` is unavailable. Pass the full URL explicitly:

```csharp
var tokens = new Dictionary<string, object>
{
    ["ActionUrl"] = $"https://{_host}/path/to/resource",
    // ...
};
```

`NotificationEmailComposer` handles this automatically for notification emails using the configured `Notifications:Email:BaseUrl`.
