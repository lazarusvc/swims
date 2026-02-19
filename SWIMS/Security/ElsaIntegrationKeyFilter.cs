using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SWIMS.Options;
using System.Security.Cryptography;
using System.Text;

namespace SWIMS.Security;

public sealed class ElsaIntegrationKeyFilter : IAsyncActionFilter, IOrderedFilter
{
    // Run before ApiController's ModelStateInvalidFilter.
    public int Order => int.MinValue;

    private readonly IOptions<ElsaOptions> _elsa;

    public ElsaIntegrationKeyFilter(IOptions<ElsaOptions> elsa)
    {
        _elsa = elsa;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var expected = (_elsa.Value.Integration.NotificationsKey ?? "").Trim();

        // ValidateOnStart already guarantees this, but keep it safe.
        if (string.IsNullOrWhiteSpace(expected))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-SWIMS-INTEGRATION-KEY", out var providedValues))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var provided = (providedValues.ToString() ?? "").Trim();

        if (!FixedTimeEquals(provided, expected))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var ab = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);

        if (ab.Length != bb.Length) return false;

        return CryptographicOperations.FixedTimeEquals(ab, bb);
    }
}
