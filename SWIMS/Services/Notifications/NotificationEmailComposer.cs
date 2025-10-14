using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using SWIMS.Services.Email; // ITemplateRenderer, EmailTemplate

namespace SWIMS.Services.Notifications;

public sealed class NotificationEmailComposer : INotificationEmailComposer
{
    private readonly IConfiguration _cfg;
    private readonly ITemplateRenderer _renderer;
    private readonly IHttpContextAccessor _http;

    public NotificationEmailComposer(IConfiguration cfg, ITemplateRenderer renderer, IHttpContextAccessor http)
    {
        _cfg = cfg;
        _renderer = renderer;
        _http = http;
    }

    public async Task<(string subject, string html, string text)> ComposeAsync(
        int userId,
        string type,
        string usernameOrEmail,
        string payloadJson,
        CancellationToken ct = default) // kept for interface parity; not used by renderer
    {
        using var doc = JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        string? fromName = root.TryGetProperty("fromName", out var fn) ? fn.GetString() : null;
        string? url = root.TryGetProperty("url", out var ue) ? ue.GetString() : null;
        string? snippet = root.TryGetProperty("snippet", out var se) ? se.GetString() : null;

        // Absolutize relative URLs using the current request (like Url.Page(..., protocol: Request.Scheme))
        string ToAbs(string? u)
        {
            if (string.IsNullOrWhiteSpace(u)) return "/";
            if (u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                u.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return u;

            var http = _http.HttpContext;
            if (http is null) return u; // background jobs without HttpContext — leave as-is
            var scheme = http.Request.Scheme;
            var host = http.Request.Host.Value;
            var path = u.StartsWith("/") ? u : "/" + u;
            return $"{scheme}://{host}{path}";
        }

        var actionUrl = ToAbs(url);

        // Prefer explicit actionLabel from payload; else we pick a good default
        var actionLabel = root.TryGetProperty("actionLabel", out var al) ? (al.GetString() ?? "") : "";

        // Subject + body intro + main text
        string subject;
        string bodyIntro = "You have a new notification in SWIMS.";
        string main = snippet ?? "";

        switch (type)
        {
            case "NewMessage":
                subject = !string.IsNullOrWhiteSpace(fromName)
                    ? $"New message from {fromName} — SWIMS"
                    : "New SWIMS chat";
                if (string.IsNullOrWhiteSpace(actionLabel)) actionLabel = "Open chat";
                if (string.IsNullOrWhiteSpace(main)) main = "You’ve received a new message.";
                break;

            default:
                subject = "New SWIMS notification";
                if (string.IsNullOrWhiteSpace(actionLabel)) actionLabel = "Open in SWIMS";
                if (string.IsNullOrWhiteSpace(main)) main = type;
                break;
        }

        // Optional small footer (if your template includes these tokens)
        var prefsUrl = ToAbs("/Portal/Notifications/Prefs");

        // Support info (from config, with safe defaults)
        var supportEmail = _cfg["Support:Email"] ?? "support@yourdomain.com";
        var supportPhone = _cfg["Support:Phone"] ?? "";

        // Tokens expected by your template engine
        var tokens = new
        {
            SubjectLine = subject,
            BodyIntro = bodyIntro,
            MainParagraph = main,
            ShowCTA = true,
            ActionLabel = actionLabel,  // short button text
            ActionUrl = actionUrl,    // absolute URL
            SupportEmail = supportEmail,
            SupportPhone = supportPhone,
            ReferenceId = Guid.NewGuid().ToString("N"),

            // Optional footer tokens (if you added them in the template)
            TypeName = type,
            PrefsUrl = prefsUrl
        };

        // Render using your 2-arg renderer; it returns EmailTemplate (Subject + HtmlBody)
        var rendered = await _renderer.RenderAsync("Notifications_Generic", tokens);

        // Use the subject we computed (template may also set it via <!-- Subject: {{SubjectLine}} -->)
        var html = rendered.HtmlBody;
        var text = $"{bodyIntro}\n\n{main}\n\n{actionUrl}";
        return (subject, html, text);
    }
}
