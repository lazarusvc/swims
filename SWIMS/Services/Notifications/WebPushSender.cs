using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using System.Text.Json;

namespace SWIMS.Services.Notifications;

public sealed class WebPushSender : IWebPushSender
{
    private readonly SwimsIdentityDbContext _db;
    private readonly PushServiceClient _client;

    public sealed class Options
    {
        public string Subject { get; set; } = "mailto:it@yourdomain.com";
        public string PublicKey { get; set; } = "";
        public string PrivateKey { get; set; } = "";
        public int DefaultTtlSeconds { get; set; } = 86_400; // 1 day
    }

    public WebPushSender(SwimsIdentityDbContext db, PushServiceClient client, IOptions<Options> opts)
    {
        _db = db;
        _client = client;

        // Default TTL for messages
        _client.DefaultTimeToLive = opts.Value.DefaultTtlSeconds;

        // Ensure default VAPID auth is configured on the client (Subject is a property, not a ctor arg)
        if (_client.DefaultAuthentication is null && !string.IsNullOrWhiteSpace(opts.Value.PublicKey))
        {
            _client.DefaultAuthentication = new VapidAuthentication(opts.Value.PublicKey, opts.Value.PrivateKey)
            {
                Subject = opts.Value.Subject
            };
            // If your package version exposes DefaultAuthenticationScheme, you may set it to WebPush:
            // _client.DefaultAuthenticationScheme = VapidAuthenticationScheme.WebPush;
        }
    }

    public async Task SendToUserAsync(int userId, object payload, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var message = new PushMessage(json); // uses client's DefaultTimeToLive & DefaultAuthentication

        var subs = await _db.PushSubscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var s in subs)
        {
            try
            {
                // Lib.Net: create empty subscription, set Endpoint and keys via SetKey(...)
                var sub = new PushSubscription
                {
                    Endpoint = s.Endpoint
                };
                sub.SetKey(PushEncryptionKeyName.P256DH, s.P256dh);
                sub.SetKey(PushEncryptionKeyName.Auth, s.Auth);

                // Uses the client's default VAPID authentication
                await _client.RequestPushMessageDeliveryAsync(sub, message, cancellationToken);
            }
            catch (PushServiceClientException ex) when ((int)ex.StatusCode == 404 || (int)ex.StatusCode == 410)
            {
                // Subscription expired/revoked → deactivate it
                var row = await _db.PushSubscriptions.FirstOrDefaultAsync(x => x.Id == s.Id, cancellationToken);
                if (row is not null)
                {
                    row.IsActive = false;
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }
            catch
            {
                // Best-effort: never let push failures break request flows
            }
        }
    }
}
