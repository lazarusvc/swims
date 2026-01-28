using Microsoft.AspNetCore.Identity;
using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.Notifications;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SWIMS.Services.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly SwimsIdentityDbContext _db;
    private readonly UserManager<SwUser> _userManager;
    private readonly RoleManager<SwRole> _roleManager;
    private readonly INotifier _notifier;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public NotificationDispatcher(
        SwimsIdentityDbContext db,
        UserManager<SwUser> userManager,
        RoleManager<SwRole> roleManager,
        INotifier notifier)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _notifier = notifier;
    }

    public async Task DispatchAsync(SwimsNotificationRequest request, CancellationToken ct = default)
    {
        if (request is null) return;
        if (string.IsNullOrWhiteSpace(request.Recipient)) return;

        // Resolve recipient (supports userId, username, email)
        var user = await ResolveUserAsync(request.Recipient);
        if (user is null) return;

        var (type, payloadJson) = BuildTypeAndPayloadJson(request);

        var username = user.UserName
            ?? user.Email
            ?? request.Recipient;

        // IMPORTANT: Notifier signature is (userId, username, type, payload)
        await _notifier.NotifyUserAsync(user.Id, username, type, payloadJson);
    }

    private async Task<SwUser?> ResolveUserAsync(string recipient)
    {
        if (int.TryParse(recipient, out var id))
            return await _userManager.FindByIdAsync(id.ToString());

        return await _userManager.FindByNameAsync(recipient)
            ?? await _userManager.FindByEmailAsync(recipient);
    }

    private static (string type, string payloadJson) BuildTypeAndPayloadJson(SwimsNotificationRequest request)
    {
        // Default type if metadata doesn't provide one yet.
        var type = "General";

        JsonObject obj = new();

        if (!string.IsNullOrWhiteSpace(request.MetadataJson))
        {
            try
            {
                var node = JsonNode.Parse(request.MetadataJson);

                if (node is JsonObject jo)
                    obj = jo;
                else if (node is not null)
                    obj["data"] = node;
            }
            catch
            {
                obj["rawMetadata"] = request.MetadataJson;
            }
        }

        // Allow metadata to drive the module-level notification Type (Cases, Forms, etc.)
        if (obj.TryGetPropertyValue("type", out var typeNode))
        {
            var t = typeNode?.GetValue<string?>();
            if (!string.IsNullOrWhiteSpace(t))
                type = t!;
        }

        // Ensure we always persist consistent fields for UI + email composer.
        obj["channel"] = request.Channel;
        obj["recipient"] = request.Recipient;
        obj["subject"] = request.Subject ?? "SWIMS update";
        obj["message"] = request.Body;

        // If your workflows include "url" and "eventKey" in MetadataJson, those will remain intact in obj.

        var payloadJson = obj.ToJsonString(_json);
        return (type, payloadJson);
    }
}
