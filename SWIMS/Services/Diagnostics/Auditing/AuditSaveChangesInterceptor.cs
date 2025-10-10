using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using System.Security.Claims;
using SWIMS.Models.Logging;

namespace SWIMS.Services.Diagnostics.Auditing;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _http;
    public AuditSaveChangesInterceptor(IHttpContextAccessor http) => _http = http;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var ctx = eventData.Context;
        if (ctx is null) return base.SavingChanges(eventData, result);

        var http = _http.HttpContext;
        var user = http?.User;
        int? userId = null;
        string? username = null;
        string? ip = null;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // Your Identity uses int keys; NameIdentifier => int
            if (int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                userId = id;
            username = user.Identity!.Name;
        }
        ip = http?.Connection?.RemoteIpAddress?.ToString();

        var entries = ctx.ChangeTracker.Entries()
            .Where(e => e.Entity is IAudited && (e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted));

        foreach (var e in entries)
        {
            var (oldJson, newJson) = SerializeDiff(e);

            var log = new AuditLog
            {
                Utc = DateTime.UtcNow,
                UserId = userId,
                Username = username,
                Ip = ip,
                Action = e.State.ToString(),
                Entity = e.Metadata.ClrType.Name,
                EntityId = TryGetPrimaryKey(e),
                OldValuesJson = oldJson,
                NewValuesJson = newJson
            };

            // Use the same context — we’re inside SaveChanges — this is acceptable for audit append
            ctx.Set<AuditLog>().Add(log);
        }

        return base.SavingChanges(eventData, result);
    }

    private static (string? oldJson, string? newJson) SerializeDiff(EntityEntry e)
    {
        string? oldJson = null, newJson = null;
        if (e.State is EntityState.Modified or EntityState.Deleted)
            oldJson = SerializeValues(e.OriginalValues);
        if (e.State is EntityState.Added or EntityState.Modified)
            newJson = SerializeValues(e.CurrentValues);
        return (oldJson, newJson);
    }

    private static string SerializeValues(PropertyValues values)
    {
        var dict = values.Properties.ToDictionary(p => p.Name, p => values[p.Name]);
        return JsonSerializer.Serialize(dict);
    }

    private static string? TryGetPrimaryKey(EntityEntry e)
        => e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();
}
