using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SWIMS.Data;
using SWIMS.Models.Notifications;
using SWIMS.Services.Notifications;
using SWIMS.Security;

namespace SWIMS.Services.Elsa;

public sealed class ElsaWorkflowJobs
{
    private readonly IElsaWorkflowClient _client;
    private readonly ILogger<ElsaWorkflowJobs> _logger;

    private readonly SwimsIdentityDbContext _db;
    private readonly INotifier _notifier;
    private readonly IMemoryCache _cache;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public ElsaWorkflowJobs(
        IElsaWorkflowClient client,
        SwimsIdentityDbContext db,
        INotifier notifier,
        IMemoryCache cache,
        ILogger<ElsaWorkflowJobs> logger)
    {
        _client = client;
        _db = db;
        _notifier = notifier;
        _cache = cache;
        _logger = logger;
    }

    [Queue("notifications")]
    [AutomaticRetry(Attempts = 0)] // avoid duplicate notifs on retry
    public async Task ExecuteByNameAsync(string workflowName, string? inputJson)
    {
        if (string.IsNullOrWhiteSpace(workflowName))
            return;

        // parse recipient for the fallback “Elsa unavailable” notification.
        var recipient = TryReadRecipient(inputJson);

        object? input = null;
        if (!string.IsNullOrWhiteSpace(inputJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(inputJson);
                input = doc.RootElement.Clone(); // safe after dispose
            }
            catch
            {
                input = inputJson; // fallback scalar
            }
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

            // IMPORTANT: request that the client throws in background context
            await _client.ExecuteByNameAsync(
                workflowName,
                input,
                cts.Token,
                throwOnUnavailable: true);
        }
        catch (ElsaWorkflowUnavailableException ex)
        {
            // Turn “TempData warning” into a real in-app System notification.
            await TryNotifyElsaUnavailableAsync(recipient, ex, CancellationToken.None);

            // Swallow: do not fail the business operation / do not retry.
            _logger.LogWarning(ex, "Elsa workflow unavailable. Workflow={WorkflowName}", workflowName);
        }
        catch (Exception ex)
        {
            // Keep job "successful" (no retries), but log for diagnostics.
            _logger.LogWarning(ex, "Elsa workflow job failed. Workflow={WorkflowName}", workflowName);
        }
    }

    private static string? TryReadRecipient(string? inputJson)
    {
        if (string.IsNullOrWhiteSpace(inputJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(inputJson);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return null;

            // Support both "recipient" and "Recipient" just in case
            if (root.TryGetProperty("recipient", out var r1) && r1.ValueKind == JsonValueKind.String)
                return r1.GetString();

            if (root.TryGetProperty("Recipient", out var r2) && r2.ValueKind == JsonValueKind.String)
                return r2.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }

	private async Task TryNotifyElsaUnavailableAsync(string? recipient, ElsaWorkflowUnavailableException ex, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(recipient))
			return;

		var resolved = await ResolveRecipientAsync(recipient, ct);
		if (resolved is null)
			return;

		var (userId, username) = resolved.Value;

		// throttle per-user to avoid spam if Elsa is down
		var throttleKey = $"elsa:unavail:{userId}";
		if (_cache.TryGetValue(throttleKey, out _))
			return;

		_cache.Set(throttleKey, true, TimeSpan.FromMinutes(5));

		var subject = "Workflow/notifications unavailable";
		var message = ex.Message;

		// Only show Hangfire link/action to admins/superadmin/perm-holders
		var canOpenHangfire = await CanAccessHangfireAsync(userId, ct);

		string? url = null;
		string? actionLabel = null;

		if (canOpenHangfire)
		{
			url = "/ops/hangfire";
			actionLabel = "Open Hangfire";
		}

		var payload = new
		{
			type = NotificationTypes.System,
			eventKey = SwimsEventKeys.System.Elsa.Unavailable,
			subject,
			message,
			snippet = message.Length > 160 ? message[..160] + "…" : message,
			url,
			actionLabel,
			metadata = new
			{
				workflow = ex.WorkflowName,
				reason = ex.Reason.ToString(),
				statusCode = ex.StatusCode
			}
		};

		await _notifier.NotifyUserAsync(
			userId: userId,
			username: username,
			type: NotificationTypes.System,
			payload: payload);
	}

	private async Task<(int userId, string username)?> ResolveRecipientAsync(string recipient, CancellationToken ct)
    {
        // Numeric id
        if (int.TryParse(recipient, out var id))
        {
            var user = await _db.Users.AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new { u.Id, Name = u.UserName ?? u.Email ?? $"User {u.Id}" })
                .FirstOrDefaultAsync(ct);

            return user is null ? null : (user.Id, user.Name);
        }

        // Username/email (normalized)
        var norm = recipient.Trim().ToUpperInvariant();

        var loginUser = await _db.Users.AsNoTracking()
            .Where(u => u.NormalizedUserName == norm || u.NormalizedEmail == norm)
            .Select(u => new { u.Id, Name = u.UserName ?? u.Email ?? $"User {u.Id}" })
            .FirstOrDefaultAsync(ct);

        return loginUser is null ? null : (loginUser.Id, loginUser.Name);
    }

	private async Task<bool> CanAccessHangfireAsync(int userId, CancellationToken ct)
	{
		var allowedRoleNames = new[]
		{
		"SuperAdmin",
		"Admin",
		$"Perm:{Permissions.Admin_Hangfire}" // => Perm:Admin.Hangfire :contentReference[oaicite:3]{index=3}
    };

		return await _db.UserRoles
			.AsNoTracking()
			.Where(ur => ur.UserId == userId)
			.Join(_db.Roles.AsNoTracking(),
				  ur => ur.RoleId,
				  r => r.Id,
				  (ur, r) => r.Name!)
			.AnyAsync(rn => allowedRoleNames.Contains(rn), ct);
	}
}