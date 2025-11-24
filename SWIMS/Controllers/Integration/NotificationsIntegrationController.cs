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

        // 2) Build a stable type for preferences + email templates
        //    For now just one bucket. Later you can use more specific types like "CaseAssigned", "ApplicationSubmitted", etc.
        var type = "WorkflowNotification";

        // 3) Build payload object (this becomes Notification.PayloadJson)
        object payload = BuildPayload(type, request);

        // 4) Let the existing notifier pipeline handle in-app + email + push based on prefs
        await _notifier.NotifyUserAsync(userId, username, type, payload);

        _logger.LogInformation(
            "Elsa notification delivered. Type={Type}, Channel={Channel}, Recipient={Recipient}",
            type, request.Channel, request.Recipient);

        return Ok(new { received = true, userId });
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

    private static object BuildPayload(string type, SwimsNotificationRequest request)
    {
        // Optional: limit snippet length
        var body = request.Body ?? "";
        var snippet = body.Length > 160 ? body[..160] + "…" : body;

        // Best-effort pass-through of metadata
        object? metadata = null;
        if (!string.IsNullOrWhiteSpace(request.MetadataJson))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<JsonElement>(request.MetadataJson);
            }
            catch
            {
                metadata = request.MetadataJson; // raw string fallback
            }
        }

        return new
        {
            subject = request.Subject,
            message = body,
            snippet,
            url = (string?)null,              // later: put deep-link here
            actionLabel = "Open in SWIMS",   // button label for email
            metadata
        };
    }
}
