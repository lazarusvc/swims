using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using SWIMS.Services.Email; // ITemplateRenderer + TemplateKeys

namespace SWIMS.Services.Notifications;

public sealed class NotificationEmailComposer : INotificationEmailComposer
{
    private readonly IConfiguration _cfg;
    private readonly ITemplateRenderer _renderer;

    public NotificationEmailComposer(IConfiguration cfg, ITemplateRenderer renderer)
    {
        _cfg = cfg;
        _renderer = renderer;
    }

    public async Task<(string subject, string html, string text)> ComposeAsync(
        int userId, string type, string usernameOrEmail, string payloadJson, CancellationToken ct = default)
    {
        var o = string.IsNullOrWhiteSpace(payloadJson)
            ? new JsonObject()
            : (JsonNode.Parse(payloadJson) as JsonObject) ?? new JsonObject();

        // Basic fields (safe defaults)
        string subject = o?["subject"]?.ToString() ?? $"SWIMS: {type}";
        string message = o?["message"]?.ToString() ?? type;
        string? actionUrl = o?["url"]?.ToString() ?? o?["actionUrl"]?.ToString();
        string actionLabel = o?["actionLabel"]?.ToString() ?? "Open in SWIMS";
        string referenceId = o?["ref"]?.ToString() ?? Guid.NewGuid().ToString("N");

        // Tokens per your templates (SubjectLine et al.) — see Email Templates v2 docs.
        var tokens = new
        {
            SubjectLine = subject,
            BodyIntro = "You have a new notification in SWIMS.",
            MainParagraph = message,
            ShowCTA = !string.IsNullOrWhiteSpace(actionUrl),
            ActionUrl = actionUrl,
            ActionLabel = actionLabel,
            SupportEmail = _cfg["Support:Email"] ?? "support@dominica.gov.dm",
            SupportPhone = _cfg["Support:Phone"] ?? "(767) 266-3310",
            ReferenceId = referenceId
        };

        // Pick template: with contacts if payload contains an array "contacts" (optional).
        var withContacts = o?["contacts"] is JsonArray arr && arr.Count > 0;
        var key = withContacts
            ? TemplateKeys.Notifications_GenericWithContacts
            : TemplateKeys.Notifications_Generic;

        // Render via your engine to get Subject + HtmlBody (module supports this).
        var rendered = await _renderer.RenderAsync(key, tokens); // returns { Subject, HtmlBody }
        // (Examples in docs show this pattern, then SendAsync(rendered.Subject, rendered.HtmlBody).) :contentReference[oaicite:7]{index=7}

        // Simple text alternative
        var text = $"{subject}\n\n{tokens.BodyIntro}\n\n{tokens.MainParagraph}"
                 + (actionUrl is null ? "" : $"\n\n{actionUrl}");

        return (rendered.Subject ?? subject, rendered.HtmlBody, text.Trim());
    }
}
