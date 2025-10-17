using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.Logging;

namespace SWIMS.Services.Diagnostics.Sessions;

public sealed class SessionLogger : ISessionLogger
{
    private readonly SwimsIdentityDbContext _db;
    public SessionLogger(SwimsIdentityDbContext db) => _db = db;

    public async Task OnSignedInAsync(int userId, string username, string sessionId, string? ip, string? userAgent)
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

    public async Task OnHeartbeatAsync(int userId, string sessionId)
    {
        var row = await _db.SessionLogs
            .OrderByDescending(x => x.LoginUtc)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SessionId == sessionId && x.LogoutUtc == null);
        if (row is null) return;
        row.LastSeenUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task OnSignedOutAsync(int userId, string sessionId)
    {
        var row = await _db.SessionLogs
            .OrderByDescending(x => x.LoginUtc)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.SessionId == sessionId && x.LogoutUtc == null);
        if (row is null) return;
        row.LogoutUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
