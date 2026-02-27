using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWIMS.Models.Notifications;
using SWIMS.Security;
using SWIMS.Services.Notifications;

namespace SWIMS.Controllers.Integration;

[ApiController]
[Route("api/integration/notifications")]
[AllowAnonymous]
[ServiceFilter(typeof(ElsaIntegrationKeyFilter))]
public sealed class NotificationsIntegrationController : ControllerBase
{
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<NotificationsIntegrationController> _logger;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public NotificationsIntegrationController(
        IBackgroundJobClient jobs,
        ILogger<NotificationsIntegrationController> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    [HttpPost("receive")]
    public IActionResult Receive([FromBody] SwimsNotificationRequest request)
    {
        if (request is null)
            return BadRequest(new { error = "missing request body" });

        try
        {
            var body = JsonSerializer.Serialize(request, _json);
            _jobs.Enqueue<NotificationDispatchJobs>(j => j.DispatchAsync(body));
            return Ok(new { ok = true, queued = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue incoming notification dispatch.");
            return StatusCode(500, new { error = "enqueue_failed" });
        }
    }
}