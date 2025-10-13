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

    // Keep these in sync with your EF mapping for log.audit_logs
    private const int MaxAction = 64;   // e.g., "ConversationCreated", "MessageSent"
    private const int MaxEntity = 128;  // e.g., "Conversation", "Message"
    private const int MaxUsername = 256;
    private const int MaxIp = 64;
    private const int MaxEntityId = 256;  // safe clamp if EntityId is limited

    private static string Cut(string? s, int max) =>
        string.IsNullOrEmpty(s) ? string.Empty : (s.Length > max ? s.Substring(0, max) : s);

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
            Username = Cut(username, MaxUsername),
            Action = Cut(action, MaxAction),
            Entity = Cut(entity, MaxEntity),
            EntityId = Cut(entityId, MaxEntityId),
            Ip = Cut(ip ?? ctx?.Connection.RemoteIpAddress?.ToString(), MaxIp),

            // JSON columns are typically nvarchar(max); no clamp needed
            OldValuesJson = oldObj is null ? null : JsonSerializer.Serialize(oldObj, JsonOpts),
            NewValuesJson = newObj is null ? null : JsonSerializer.Serialize(newObj, JsonOpts),
            ExtraJson = extra is null ? null : JsonSerializer.Serialize(extra, JsonOpts),
        };

        _db.AuditLogs.Add(row);
        await _db.SaveChangesAsync(ct);
    }
}
