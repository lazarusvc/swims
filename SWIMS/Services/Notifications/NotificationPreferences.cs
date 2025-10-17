using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.Notifications;

namespace SWIMS.Services.Notifications;

public sealed class NotificationPreferences : INotificationPreferences
{
    private readonly SwimsIdentityDbContext _db;
    public NotificationPreferences(SwimsIdentityDbContext db) => _db = db;

    public async Task<(bool inApp, bool email, bool digest)> GetEffectiveAsync(int userId, string type, CancellationToken ct = default)
    {
        // Type-specific
        var row = await _db.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Type == type, ct);
        if (row != null) return (row.InAppEnabled, row.EmailEnabled, row.DigestEnabled);

        // Global default for the user
        var def = await _db.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Type == null, ct);
        if (def != null) return (def.InAppEnabled, def.EmailEnabled, def.DigestEnabled);

        // System defaults
        return (inApp: true, email: false, digest: false);
    }

    public async Task UpsertAsync(int userId, string? type, bool inApp, bool email, bool digest, CancellationToken ct = default)
    {
        var row = await _db.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Type == type, ct);

        if (row is null)
        {
            row = new NotificationPreference
            {
                UserId = userId,
                Type = type,
                InAppEnabled = inApp,
                EmailEnabled = email,
                DigestEnabled = digest
            };
            _db.NotificationPreferences.Add(row);
        }
        else
        {
            row.InAppEnabled = inApp;
            row.EmailEnabled = email;
            row.DigestEnabled = digest;
            row.UpdatedUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<(string? type, bool inApp, bool email, bool digest)>> ListAsync(int userId, CancellationToken ct = default)
    {
        var list = await _db.NotificationPreferences
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Type == null ? 0 : 1).ThenBy(x => x.Type)
            .Select(x => new { x.Type, x.InAppEnabled, x.EmailEnabled, x.DigestEnabled })
            .ToListAsync(ct);

        return list.Select(x => (x.Type, x.InAppEnabled, x.EmailEnabled, x.DigestEnabled)).ToList();
    }
}
