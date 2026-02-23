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
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<NotificationsIntegrationController> _logger;

    public NotificationsIntegrationController(
        INotificationDispatcher dispatcher,
        ILogger<NotificationsIntegrationController> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] SwimsNotificationRequest request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest(new { error = "missing request body" });

        try
        {
            await _dispatcher.DispatchAsync(request, ct);
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch incoming notification.");
            return StatusCode(500, new { error = "dispatch_failed" });
        }
    }
}