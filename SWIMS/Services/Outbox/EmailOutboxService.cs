using Microsoft.EntityFrameworkCore;
using Serilog;
using SWIMS.Data;
using SWIMS.Models.Email;
using SWIMS.Models.Outbox;
using SWIMS.Services.Email;

namespace SWIMS.Services.Outbox;

public sealed class EmailOutboxService : IEmailOutbox
{
    private readonly SwimsIdentityDbContext _db;
    private readonly IEmailService _emails;

    public EmailOutboxService(SwimsIdentityDbContext db, IEmailService emails)
    {
        _db = db;
        _emails = emails;
    }

    public async Task<Guid> EnqueueAsync(
        string to, string subject, string? html, string? text = null,
        string? cc = null, string? bcc = null, string? headersJson = null)
    {
        var row = new EmailOutbox
        {
            To = to,
            Subject = subject,
            BodyHtml = html,
            BodyText = text,
            Cc = cc,
            Bcc = bcc,
            HeadersJson = headersJson,
            Attempts = 0,
            NextAttemptUtc = DateTime.UtcNow
        };

        _db.EmailOutbox.Add(row);
        await _db.SaveChangesAsync();
        return row.Id;
    }

    public async Task<int> DispatchBatchAsync(int take, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var items = await _db.EmailOutbox
            .Where(x => x.SentUtc == null && (x.NextAttemptUtc == null || x.NextAttemptUtc <= now))
            .OrderBy(x => x.CreatedUtc)
            .Take(take)
            .ToListAsync(ct);

        var sent = 0;
        foreach (var m in items)
        {
            try
            {
                var to = new EmailAddress(m.To);
                var cc = ParseAddresses(m.Cc);
                var bcc = ParseAddresses(m.Bcc);
                var body = m.BodyHtml ?? m.BodyText ?? string.Empty;

                await _emails.SendAsync(to, m.Subject, body, from: null, cc: cc, bcc: bcc, attachments: null, ct: ct);

                m.SentUtc = DateTime.UtcNow;
                sent++;
            }
            catch (Exception ex)
            {
                m.Attempts++;
                m.LastError = ex.Message;

                // Exponential-ish backoff: 5m * attempts (capped at 1h)
                var delay = TimeSpan.FromMinutes(Math.Min(60, m.Attempts * 5));
                m.NextAttemptUtc = DateTime.UtcNow + delay;

                if (m.Attempts >= 10)
                {
                    _db.EmailDeadLetters.Add(new EmailDeadLetter
                    {
                        Id = m.Id,
                        To = m.To,
                        Subject = m.Subject,
                        BodyHtml = m.BodyHtml,
                        BodyText = m.BodyText,
                        HeadersJson = m.HeadersJson,
                        Attempts = m.Attempts,
                        CreatedUtc = m.CreatedUtc,
                        Error = ex.ToString()
                    });
                    _db.EmailOutbox.Remove(m);
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        if (sent > 0) Log.Information("EmailOutbox dispatched {Count} messages", sent);
        return sent;
    }

    private static IEnumerable<EmailAddress>? ParseAddresses(string? list)
    {
        if (string.IsNullOrWhiteSpace(list)) return null;
        return list
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => new EmailAddress(s));
    }
}
