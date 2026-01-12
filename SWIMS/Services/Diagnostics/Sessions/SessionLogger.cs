using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWIMS.Data;
using SWIMS.Models.Logging;

namespace SWIMS.Services.Diagnostics.Sessions;

public sealed class SessionLogger : ISessionLogger
{
    private readonly SwimsIdentityDbContext _db;
    private readonly ILogger<SessionLogger> _logger;

    public SessionLogger(SwimsIdentityDbContext db, ILogger<SessionLogger> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task OnSignedInAsync(int userId, string username, string sessionId, string? ip, string? userAgent)
    {
        try
        {
            var row = new SessionLog
            {
                UserId = userId,
                Username = username,
                SessionId = sessionId,
                Ip = ip,
                UserAgent = userAgent
            };
            _db.SessionLogs.Add(row);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don’t block login because session logging failed
            _logger.LogWarning(ex, "SessionLogger.OnSignedInAsync failed for user {UserId}, session {SessionId}", userId, sessionId);
        }
    }

    public async Task OnHeartbeatAsync(int userId, string sessionId)
    {
        try
        {
            var row = await _db.SessionLogs
                .Where(x => x.UserId == userId && x.SessionId == sessionId && x.LogoutUtc == null)
                .OrderByDescending(x => x.LoginUtc)
                .FirstOrDefaultAsync();

            if (row is null) return;

            row.LastSeenUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Heartbeat should never take the app down.
            _logger.LogWarning(ex, "SessionLogger.OnHeartbeatAsync failed for user {UserId}, session {SessionId}", userId, sessionId);
        }
    }

    public async Task OnSignedOutAsync(int userId, string sessionId)
    {
        try
        {
            var row = await _db.SessionLogs
                .Where(x => x.UserId == userId && x.SessionId == sessionId && x.LogoutUtc == null)
                .OrderByDescending(x => x.LoginUtc)
                .FirstOrDefaultAsync();

            if (row is null) return;

            row.LogoutUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Don’t block logout if logging fails
            _logger.LogWarning(ex, "SessionLogger.OnSignedOutAsync failed for user {UserId}, session {SessionId}", userId, sessionId);
        }
    }
}
