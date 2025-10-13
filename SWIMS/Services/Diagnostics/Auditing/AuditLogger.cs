using Microsoft.AspNetCore.Http;
using SWIMS.Data;
using SWIMS.Models.Logging;
using System.Text.Json;

namespace SWIMS.Services.Diagnostics.Auditing;

public sealed class AuditLogger : IAuditLogger
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IHttpContextAccessor _http;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public AuditLogger(SwimsIdentityDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(
        string action, string entity, string? entityId,
        int userId, string username,
        object? oldObj = null, object? newObj = null,
        string? ip = null, object? extra = null,
        CancellationToken ct = default)
    {
        var ctx = _http.HttpContext;
        var row = new AuditLog
        {
            Utc = DateTime.UtcNow,
            UserId = userId,
            Username = username,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Ip = ip ?? ctx?.Connection.RemoteIpAddress?.ToString(),
            OldValuesJson = oldObj is null ? null : JsonSerializer.Serialize(oldObj, JsonOpts),
            NewValuesJson = newObj is null ? null : JsonSerializer.Serialize(newObj, JsonOpts),
            ExtraJson = extra is null ? null : JsonSerializer.Serialize(extra, JsonOpts),
        };

        _db.AuditLogs.Add(row);
        await _db.SaveChangesAsync(ct);
    }
}
