using System.Security.Claims;

namespace SWIMS.Services.Diagnostics.Auditing;

public static class AuditHelpers
{
    public static bool TryResolveActor(ClaimsPrincipal user, out int userId, out string username)
    {
        username = user?.Identity?.Name ?? "unknown";
        userId = 0;

        var idStr = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out userId);
    }

    public static async Task TryLogAsync(
        this IAuditLogger audit,
        string action,
        string entity,
        string? entityId,
        int userId,
        string username,
        object? oldObj = null,
        object? newObj = null,
        object? extra = null,
        CancellationToken ct = default)
    {
        try
        {
            await audit.LogAsync(
                action: action,
                entity: entity,
                entityId: entityId,
                userId: userId,
                username: username,
                oldObj: oldObj,
                newObj: newObj,
                ip: null,
                extra: extra,
                ct: ct);
        }
        catch
        {
            // best-effort for launch stability
        }
    }
}