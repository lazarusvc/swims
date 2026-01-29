using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SWIMS.Data;
using SWIMS.Models;
using SWIMS.Models.Notifications;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SWIMS.Services.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private const string SuperAdminRoleName = "SuperAdmin";

    private readonly SwimsIdentityDbContext _db;
    private readonly UserManager<SwUser> _userManager;
    private readonly RoleManager<SwRole> _roleManager;
    private readonly INotifier _notifier;

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

        // Parse envelope: { type, eventKey, url, metadata:{...} }
        var defaultType = NotificationTypes.System;
        var (type, eventKey, url, innerMetadata, rawMetadataNode) = ParseEnvelope(request.MetadataJson, defaultType);

        // Collect recipients (dedupe)
        var recipientIds = new HashSet<int>();

        // Primary recipient (actor / explicit recipient)
        if (!string.IsNullOrWhiteSpace(request.Recipient))
        {
            var primaryId = await ResolveRecipientUserIdAsync(request.Recipient);
            if (primaryId.HasValue)
                recipientIds.Add(primaryId.Value);
        }

        // Default extras (your requested pattern)
        foreach (var id in ExtractTargetUserIds(innerMetadata, rawMetadataNode))
        recipientIds.Add(id);

        // Fan-out from routing table (only if we have an eventKey)
        if (!string.IsNullOrWhiteSpace(eventKey))
        {
            await AddRouteRecipientsAsync(eventKey!, recipientIds, ct);
            await AddSuperAdminRecipientsAsync(recipientIds, ct);
        }

        if (recipientIds.Count == 0)
            return;

        // Resolve usernames for all recipients (so persisted notifications have correct Username)
        var userNameMap = await _db.Users
            .AsNoTracking()
            .Where(u => recipientIds.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                Name = u.UserName ?? u.Email ?? $"User {u.Id}"
            })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        // Send once per resolved recipient
        foreach (var userId in recipientIds)
        {
            var (finalType, payloadJson) = BuildPayloadJson(
                type: type,
                eventKey: eventKey,
                url: url,
                request: request,
                metadata: innerMetadata ?? rawMetadataNode,
                recipient: userId.ToString());

            var username = userNameMap.TryGetValue(userId, out var name)
                 ? name
                 : $"User {userId}";
            
            await _notifier.NotifyUserAsync(
                userId: userId,
                username: username,
                type: finalType,
                payload: payloadJson);
        }
    }

    private async Task<int?> ResolveRecipientUserIdAsync(string recipient)
    {
        if (int.TryParse(recipient, out var id))
        {
            var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == id);
            return exists ? id : null;
        }

        var norm = recipient.Trim().ToUpperInvariant();
        var user = await _db.Users.AsNoTracking()
            .Where(u => u.NormalizedUserName == norm || u.NormalizedEmail == norm)
            .Select(u => new { u.Id })
            .FirstOrDefaultAsync();

        return user?.Id;
    }

    private static (string Type, string? EventKey, string? Url, JsonNode? InnerMetadata, JsonNode? RawNode)
        ParseEnvelope(string? metadataJson, string defaultType)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return (defaultType, null, null, null, null);

        try
        {
            var node = JsonNode.Parse(metadataJson);

            if (node is not JsonObject obj)
                return (defaultType, null, null, node, node);

            var type = ReadString(obj, "type") ?? defaultType;
            var eventKey = ReadString(obj, "eventKey");
            var url = ReadString(obj, "url");

            // Prefer inner "metadata" object, but don’t require it.
            obj.TryGetPropertyValue("metadata", out var inner);

            return (type, eventKey, url, inner, node);
        }
        catch
        {
            // Invalid JSON: keep as raw
            return (defaultType, null, null, null, JsonValue.Create(metadataJson));
        }

        static string? ReadString(JsonObject obj, string prop)
        {
            if (!obj.TryGetPropertyValue(prop, out var n) || n is null) return null;
            try { return n.GetValue<string>(); } catch { return null; }
        }
    }

    private static IEnumerable<int> ExtractTargetUserIds(JsonNode? inner, JsonNode? raw)
    {
        // We look in both inner metadata and raw root (back-compat / safety),
        // but your canonical location should be: root.metadata.targetUserId(s)
        foreach (var id in ExtractFromNode(inner))
            yield return id;

        foreach (var id in ExtractFromNode(raw))
            yield return id;

        static IEnumerable<int> ExtractFromNode(JsonNode? node)
        {
            if (node is not JsonObject obj) yield break;

            // targetUserId
            if (TryReadInt(obj, "targetUserId", out var single))
                yield return single;

            // targetUserIds
            if (obj.TryGetPropertyValue("targetUserIds", out var idsNode) && idsNode is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    if (TryParseIntNode(item, out var v))
                        yield return v;
                }
            }
        }

        static bool TryReadInt(JsonObject obj, string prop, out int value)
        {
            value = 0;
            if (!obj.TryGetPropertyValue(prop, out var n) || n is null) return false;
            return TryParseIntNode(n, out value);
        }

        static bool TryParseIntNode(JsonNode? node, out int value)
        {
            value = 0;
            if (node is null) return false;

            try
            {
                // number
                value = node.GetValue<int>();
                return true;
            }
            catch { /* ignore */ }

            try
            {
                // string
                var s = node.GetValue<string>();
                return int.TryParse(s, out value);
            }
            catch { /* ignore */ }

            return false;
        }
    }

    private async Task AddRouteRecipientsAsync(string eventKey, HashSet<int> recipients, CancellationToken ct)
    {
        var route = await _db.NotificationRoutes
            .Include(r => r.Users)
            .Include(r => r.Roles)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EventKey == eventKey, ct);

        if (route is null || !route.IsEnabled)
            return;

        foreach (var u in route.Users)
            recipients.Add(u.UserId);

        var roleIds = route.Roles.Select(r => r.RoleId).Distinct().ToList();
        if (roleIds.Count == 0) return;

        var roleUserIds = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => roleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var id in roleUserIds)
            recipients.Add(id);
    }

    private async Task AddSuperAdminRecipientsAsync(HashSet<int> recipients, CancellationToken ct)
    {
        var superRole = await _roleManager.FindByNameAsync(SuperAdminRoleName);
        if (superRole is null) return;

        var superIds = await _db.Set<IdentityUserRole<int>>()
            .AsNoTracking()
            .Where(ur => ur.RoleId == superRole.Id)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var id in superIds)
            recipients.Add(id);
    }


    private static (string Type, string PayloadJson) BuildPayloadJson(
        string type,
        string? eventKey,
        string? url,
        SwimsNotificationRequest request,
        JsonNode? metadata,
        string recipient)
    {
        var body = request.Body ?? "";
        var snippet = body.Length > 160 ? body[..160] + "…" : body;

        var payloadObj = new
        {
            type,
            eventKey,
            recipient,
            subject = request.Subject,
            message = body,
            snippet,
            url,
            actionLabel = "Open in SWIMS",
            metadata
        };

        return (type, JsonSerializer.Serialize(payloadObj));
    }
}
