using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;

namespace SWIMS.Controllers.Integration;

[ApiController]
[Route("api/integration/notifications")]
[AllowAnonymous] // dev-only – later we’ll lock this down with an internal key
public class NotificationsIntegrationController : ControllerBase
{
    private readonly ILogger<NotificationsIntegrationController> _logger;
    private readonly SwimsIdentityDbContext _db;
    private readonly INotifier _notifier;

    public NotificationsIntegrationController(
        ILogger<NotificationsIntegrationController> logger,
        SwimsIdentityDbContext db,
        INotifier notifier)
    {
        _logger = logger;
        _db = db;
        _notifier = notifier;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] SwimsNotificationRequest request,
        CancellationToken ct)
    {
        // 1) Resolve user by Id OR login (username/email)
        if (!TryParseUser(request.Recipient, out var userId, out var username, out var error))
        {
            _logger.LogWarning("Elsa notification: {Error}. Recipient={Recipient}", error, request.Recipient);
            return BadRequest(new { error });
        }

        // 2) Preferences "type" should be stable & small (module/category-level).
        //    Default remains backward-compatible.
        const string defaultType = "WorkflowNotification";

        // 3) Extract metadata (best-effort) and derive type/eventKey/url for auditing + deep links.
        var parsed = ParseMetadata(request.MetadataJson, defaultType);
        var type = parsed.Type;
        var eventKey = parsed.EventKey;
        var url = parsed.Url;

        // 4) Build payload object (this becomes Notification.PayloadJson)
        object payload = BuildPayload(type, eventKey, url, request, parsed.Metadata);

        // 5) Let the existing notifier pipeline handle in-app + email + push based on prefs
        await _notifier.NotifyUserAsync(userId, username, type, payload);

        _logger.LogInformation(
            "Elsa notification delivered. Type={Type}, EventKey={EventKey}, Channel={Channel}, Recipient={Recipient}",
            type, eventKey, request.Channel, request.Recipient);

        return Ok(new { received = true, userId, type, eventKey });
    }

    private bool TryParseUser(
        string recipient,
        out int userId,
        out string username,
        out string error)
    {
        userId = 0;
        username = "";
        error = "";

        // Numeric Id
        if (int.TryParse(recipient, out var id))
        {
            var user = _db.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new { u.Id, u.UserName, u.Email })
                .FirstOrDefault();

            if (user is null)
            {
                error = $"userId {id} not found";
                return false;
            }

            userId = user.Id;
            username = user.UserName ?? user.Email ?? $"user:{user.Id}";
            return true;
        }

        // Username / email – normalize like MessagingEndpoints.Norm
        var norm = recipient.Trim().ToUpperInvariant();

        var loginUser = _db.Users
            .AsNoTracking()
            .Where(u => u.NormalizedUserName == norm || u.NormalizedEmail == norm)
            .Select(u => new { u.Id, u.UserName, u.Email })
            .FirstOrDefault();

        if (loginUser is null)
        {
            error = $"login '{recipient}' not found";
            return false;
        }

        userId = loginUser.Id;
        username = loginUser.UserName ?? loginUser.Email ?? $"user:{loginUser.Id}";
        return true;
    }

    private sealed record ParsedMetadata(
        string Type,
        string? EventKey,
        string? Url,
        object? Metadata);

    private static ParsedMetadata ParseMetadata(string? metadataJson, string defaultType)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return new ParsedMetadata(defaultType, null, null, null);

        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return new ParsedMetadata(defaultType, null, null, metadataJson);

            var type = defaultType;
            if (root.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
            {
                var s = typeProp.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    type = s!;
            }

            string? eventKey = null;
            if (root.TryGetProperty("eventKey", out var eventProp) && eventProp.ValueKind == JsonValueKind.String)
                eventKey = eventProp.GetString();

            string? url = null;
            if (root.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String)
                url = urlProp.GetString();

            // Clone the JsonElement so it survives after JsonDocument is disposed.
            var metadata = root.Clone();

            return new ParsedMetadata(type, eventKey, url, metadata);
        }
        catch
        {
            // Raw string fallback if JSON is invalid
            return new ParsedMetadata(defaultType, null, null, metadataJson);
        }
    }

    private static object BuildPayload(
        string type,
        string? eventKey,
        string? url,
        SwimsNotificationRequest request,
        object? metadata)
    {
        // Optional: limit snippet length
        var body = request.Body ?? "";
        var snippet = body.Length > 160 ? body[..160] + "…" : body;

        return new
        {
            // Helpful for audit/search even if your DB has a Type column too.
            type,

            // Fine-grained audit key (e.g. "Swims.Events.Cases.Assigned")
            eventKey,

            subject = request.Subject,
            message = body,
            snippet,

            // Allow deep-linking from dropdown/toast/email when SWIMS supplies it.
            url,

            // Button label for email templates if/when re-enabled
            actionLabel = "Open in SWIMS",

            // Metadata passthrough (json object or raw string)
            metadata
        };
    }
}
