using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SWIMS.Models.ViewModels;

namespace SWIMS.Controllers;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ErrorController : Controller
{
    // Used by app.UseExceptionHandler("/Error/")
    [HttpGet("/Error/")]
    public IActionResult Index()
    {
        var _ = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var model = new ErrorPageViewModel
        {
            StatusCode = 500,
            Title = "Server Error",
            Message = "Something went wrong on our end.",
            RequestId = requestId
        };

        Response.StatusCode = 500;
        return View("ServerError", model);
    }

    // Used by app.UseStatusCodePagesWithReExecute("/Error/{0}/")
    [HttpGet("/Error/{statusCode:int}/")]
    public IActionResult Status(int statusCode)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var (viewName, title, message) = statusCode switch
        {
            400 => ("BadRequest", "Bad Request", "Sorry, the request could not be understood."),
            401 => ("Unauthorized", "Unauthorized", "You may need to sign in to continue."),
            403 => ("Forbidden", "Access Denied", "You don’t have permission to access this resource."),
            404 => ("NotFound", "Page not Found", "Sorry, the page you are looking for doesn’t exist."),
            408 => ("RequestTimeout", "Request Timeout", "The request took too long. Please try again."),
            413 => ("PayloadTooLarge", "Payload Too Large", "The uploaded content is too large."),
            429 => ("TooManyRequests", "Too Many Requests", "You’re doing that too often. Please slow down and try again."),
            500 => ("ServerError", "Server Error", "Something went wrong on our end."),
            502 => ("BadGateway", "Bad Gateway", "We received an invalid response from an upstream service."),
            503 => ("ServiceUnavailable", "Service Unavailable", "The service is temporarily unavailable. Please try again later."),
            504 => ("GatewayTimeout", "Gateway Timeout", "The server took too long to respond. Please try again."),
            _ => ("StatusCode", $"Error {statusCode}", "An error occurred while processing your request.")
        };

        var model = new ErrorPageViewModel
        {
            StatusCode = statusCode,
            Title = title,
            Message = message,
            RequestId = requestId
        };

        Response.StatusCode = statusCode;
        return View(viewName, model);
    }
}
