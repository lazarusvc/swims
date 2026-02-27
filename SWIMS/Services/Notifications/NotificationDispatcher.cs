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

    private enum Audience
    {
        Actor,
        Target,
        Routed,
        SuperAdmin,
        Other
    }

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
        var metadataForPayload = innerMetadata ?? rawMetadataNode;

        // Groups we need for audience-specific wording
        int? primaryId = null;
        var targetIds = new HashSet<int>(ExtractTargetUserIds(innerMetadata, rawMetadataNode));
        var routeIds = new HashSet<int>();
        var superIds = new HashSet<int>();

        // Collect recipients (dedupe)
        var recipientIds = new HashSet<int>();

        // Primary recipient (actor / explicit recipient)
        if (!string.IsNullOrWhiteSpace(request.Recipient))
        {
            primaryId = await ResolveRecipientUserIdAsync(request.Recipient);
            if (primaryId.HasValue)
                recipientIds.Add(primaryId.Value);
        }

        // Targets
        foreach (var id in targetIds)
            recipientIds.Add(id);

        // Fan-out from routing table + superadmins (only if we have an eventKey)
        if (!string.IsNullOrWhiteSpace(eventKey))
        {
            routeIds = await ResolveRouteRecipientsAsync(type, eventKey!, ct);
            foreach (var id in routeIds) recipientIds.Add(id);

            superIds = await ResolveSuperAdminRecipientsAsync(ct);
            foreach (var id in superIds) recipientIds.Add(id);
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
            var audience = DetermineAudience(userId, primaryId, targetIds, routeIds, superIds);

            // ✅ Audience-specific subject/body overrides (optional)
            var (subject, body) = ResolveAudienceText(
                metadataForPayload,
                audience,
                request.Subject ?? "",
                request.Body ?? "");

            var (finalType, payloadJson) = BuildPayloadJson(
                type: type,
                eventKey: eventKey,
                url: url,
                subject: subject,
                body: body,
                metadata: metadataForPayload,
                recipient: userId.ToString(),
                audience: audience.ToString());

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

    private static Audience DetermineAudience(
        int userId,
        int? primaryId,
        HashSet<int> targetIds,
        HashSet<int> routeIds,
        HashSet<int> superIds)
    {
        // Priority order matters:
        // Target > Actor > SuperAdmin > Routed > Other
        if (targetIds.Contains(userId)) return Audience.Target;
        if (primaryId.HasValue && primaryId.Value == userId) return Audience.Actor;
        if (superIds.Contains(userId)) return Audience.SuperAdmin;
        if (routeIds.Contains(userId)) return Audience.Routed;
        return Audience.Other;
    }

    private static (string subject, string body) ResolveAudienceText(
        JsonNode? metadataNode,
        Audience audience,
        string defaultSubject,
        string defaultBody)
    {
        if (metadataNode is not JsonObject metaObj)
            return (defaultSubject, defaultBody);

        // Expect:
        // metadata: { ..., texts: { actor:{subject,body}, target:{...}, routed:{...}, superadmin:{...} } }
        if (!metaObj.TryGetPropertyValue("texts", out var textsNode) || textsNode is not JsonObject textsObj)
            return (defaultSubject, defaultBody);

        string key = audience switch
        {
            Audience.Actor => "actor",
            Audience.Target => "target",
            Audience.Routed => "routed",
            Audience.SuperAdmin => "superadmin",
            _ => "other"
        };

        // Try exact audience key; fall back to "default" if you ever add it later
        if (!textsObj.TryGetPropertyValue(key, out var audienceNode) || audienceNode is not JsonObject audienceObj)
        {
            if (!textsObj.TryGetPropertyValue("default", out var defNode) || defNode is not JsonObject defObj)
                return (defaultSubject, defaultBody);

            return ReadText(defObj, defaultSubject, defaultBody);
        }

        return ReadText(audienceObj, defaultSubject, defaultBody);

        static (string subject, string body) ReadText(JsonObject obj, string s0, string b0)
        {
            var subj = TryReadString(obj, "subject") ?? s0;
            var body = TryReadString(obj, "body") ?? TryReadString(obj, "message") ?? b0;
            return (subj, body);
        }

        static string? TryReadString(JsonObject obj, string prop)
        {
            if (!obj.TryGetPropertyValue(prop, out var n) || n is null) return null;
            try { return n.GetValue<string>(); } catch { return null; }
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

            obj.TryGetPropertyValue("metadata", out var inner);

            return (type, eventKey, url, inner, node);
        }
        catch
        {
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
        foreach (var id in ExtractFromNode(inner))
            yield return id;

        foreach (var id in ExtractFromNode(raw))
            yield return id;

        static IEnumerable<int> ExtractFromNode(JsonNode? node)
        {
            if (node is not JsonObject obj) yield break;

            if (TryReadInt(obj, "targetUserId", out var single))
                yield return single;

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
                value = node.GetValue<int>();
                return true;
            }
            catch { }

            try
            {
                var s = node.GetValue<string>();
                return int.TryParse(s, out value);
            }
            catch { }

            return false;
        }
    }

    private async Task<HashSet<int>> ResolveRouteRecipientsAsync(string type, string eventKey, CancellationToken ct)
    {
        var result = new HashSet<int>();

        // 1) Find the route row (no Includes; only scalar projection)
        var routeRow = await _db.NotificationRoutes
            .AsNoTracking()
            .Where(r => r.EventKey == eventKey && r.Type == type)
            .Select(r => new { r.Id, r.IsEnabled })
            .FirstOrDefaultAsync(ct);

        // fallback for legacy/dirty rows
        if (routeRow is null)
        {
            routeRow = await _db.NotificationRoutes
                .AsNoTracking()
                .Where(r => r.EventKey == eventKey)
                .OrderByDescending(r => r.UpdatedAtUtc)
                .Select(r => new { r.Id, r.IsEnabled })
                .FirstOrDefaultAsync(ct);
        }

        if (routeRow is null || !routeRow.IsEnabled)
            return result;

        var routeId = routeRow.Id;

        // 2) Explicit users
        var directUserIds = await _db.Set<NotificationRouteUser>()
            .AsNoTracking()
            .Where(x => x.RouteId == routeId)
            .Select(x => x.UserId)
            .ToListAsync(ct);

        foreach (var id in directUserIds)
            result.Add(id);

        // 3) Permission expansion (policy -> policy_roles -> user_roles)
        var permKeys = await _db.Set<NotificationRoutePermission>()
            .AsNoTracking()
            .Where(x => x.RouteId == routeId)
            .Select(x => x.PermissionKey)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToListAsync(ct);

        if (permKeys.Count > 0)
        {
            var permUserIds = await ResolveUsersByPermissionsAsync(permKeys, ct);
            foreach (var id in permUserIds)
                result.Add(id);
        }

        // 4) Role expansion
        var roleIds = await _db.Set<NotificationRouteRole>()
            .AsNoTracking()
            .Where(x => x.RouteId == routeId)
            .Select(x => x.RoleId)
            .Distinct()
            .ToListAsync(ct);

        if (roleIds.Count > 0)
        {
            var roleUserIds = await _db.UserRoles
                .AsNoTracking()
                .Where(ur => roleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync(ct);

            foreach (var id in roleUserIds)
                result.Add(id);
        }

        return result;
    }

    private async Task<List<int>> ResolveUsersByPermissionsAsync(List<string> permissionKeys, CancellationToken ct)
    {
        // 1) Find enabled policies by name (permission key)
        var policyIds = await _db.AuthorizationPolicies
            .AsNoTracking()
            .Where(p => p.IsEnabled && permissionKeys.Contains(p.Name))
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (policyIds.Count == 0)
            return new List<int>();

        // 2) Get role ids attached to those policies
        var roleIds = await _db.AuthorizationPolicyRoles
            .AsNoTracking()
            .Where(pr => policyIds.Contains(pr.AuthorizationPolicyEntityId))
            .Select(pr => pr.RoleId)
            .Distinct()
            .ToListAsync(ct);

        if (roleIds.Count == 0)
            return new List<int>();

        // 3) Expand to users in those roles
        return await _db.UserRoles
            .AsNoTracking()
            .Where(ur => roleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);
    }


    private async Task<HashSet<int>> ResolveSuperAdminRecipientsAsync(CancellationToken ct)
    {
        var result = new HashSet<int>();

        var superRole = await _roleManager.FindByNameAsync(SuperAdminRoleName);
        if (superRole is null) return result;

        var superIds = await _db.Set<IdentityUserRole<int>>()
            .AsNoTracking()
            .Where(ur => ur.RoleId == superRole.Id)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var id in superIds)
            result.Add(id);

        return result;
    }

    private static (string Type, string PayloadJson) BuildPayloadJson(
        string type,
        string? eventKey,
        string? url,
        string subject,
        string body,
        JsonNode? metadata,
        string recipient,
        string? audience)
    {
        var snippet = body.Length > 160 ? body[..160] + "…" : body;

        var payloadObj = new
        {
            type,
            eventKey,
            recipient,
            subject,
            message = body,
            snippet,
            url,
            actionLabel = "Open in SWIMS",
            audience,
            metadata
        };

        return (type, JsonSerializer.Serialize(payloadObj));
    }
}
