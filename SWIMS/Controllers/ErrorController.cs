using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using SWIMS.Models;
using SWIMS.Models.ViewModels;
using SWIMS.Security;

namespace SWIMS.Controllers;

[AllowAnonymous]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ErrorController : Controller
{
    private readonly SwimsDb_moreContext _db;
    private readonly IAuthorizationService _authorization;

    public ErrorController(SwimsDb_moreContext db, IAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    // Used by app.UseExceptionHandler("/Error/")
    [HttpGet("/Error")]
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

    

    [HttpGet("/Error/{statusCode:int}")]
    public async Task<IActionResult> Status(int statusCode)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var re = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var originalPath = re?.OriginalPath ?? string.Empty;
        var originalQuery = re?.OriginalQueryString ?? string.Empty;

        // Smart 403 only for the ApprovalAction endpoint
        if (statusCode == 403 && IsApprovalActionPath(originalPath))
        {
            var approvalModel = await BuildApprovalBlockedModelAsync(originalPath, originalQuery, requestId);
            Response.StatusCode = 403;
            return View("ApprovalBlocked", approvalModel);
        }

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
            504 => ("GatewayTimeout", "Gateway Timeout", "The server took too long to respond."),
            _ => ("Status", "Something went wrong", "An unexpected error occurred.")
        };

        var pageModel = new ErrorPageViewModel
        {
            StatusCode = statusCode,
            Title = title,
            Message = message,
            RequestId = requestId
        };

        Response.StatusCode = statusCode;
        return View(viewName, pageModel);
    }

    private static bool IsApprovalActionPath(string path)
        => path.Equals("/form/ApprovalAction", StringComparison.OrdinalIgnoreCase);

    private async Task<ApprovalBlockedViewModel> BuildApprovalBlockedModelAsync(string originalPath, string originalQuery, string requestId)
    {
        var q = QueryHelpers.ParseQuery(originalQuery ?? string.Empty);

        int? dataId =
            TryGetInt(q, "dataID") ??
            TryGetInt(q, "dataId") ??
            TryGetInt(q, "id") ??
            TryGetInt(q, "Id");

        int? attemptedLevel = TryGetInt(q, "appCnt");
        string? uuid = TryGetString(q, "uuid");

        var userLevels = await GetUserApprovalLevelsAsync();

        var model = new ApprovalBlockedViewModel
        {
            StatusCode = 403,
            RequestId = requestId,
            OriginalPath = originalPath,
            OriginalQueryString = originalQuery,
            DataId = dataId,
            AttemptedApprovalLevel = attemptedLevel,
            UserApprovalLevels = userLevels,
            ApprovalsDashboardUrl = Url.Action("Index", "Approvals"),
            ApprovalsReturnUrl = string.IsNullOrWhiteSpace(uuid) ? null : Url.Action("Approval", "form", new { uuid }),
        };

        if (dataId is null)
        {
            model.Title = "This approval item isn’t available yet";
            model.Message = "This approval link is missing an item ID, so the system can’t determine the current approval stage.";
            model.Hint = "Go back to the approvals queue and open the item again.";
            return model;
        }

        var data = await _db.SW_formTableData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dataId.Value);

        if (data is null)
        {
            model.Title = "Approval item not found";
            model.Message = "The approval item you tried to open doesn’t exist (it may have been deleted).";
            model.Hint = "Go back to the approvals queue and refresh.";
            return model;
        }

        // Pull approval configuration from the form (approvalAmt)
        var form = await _db.SW_forms
            .AsNoTracking()
            .Where(f => f.Id == data.SW_formsId)
            .Select(f => new { f.Id, f.name, f.uuid, f.approvalAmt })
            .FirstOrDefaultAsync();

        int configured = form?.approvalAmt ?? 0;
        if (configured < 0) configured = 0;
        if (configured > 5) configured = 5;

        model.FormId = form?.Id;
        model.FormName = form?.name;
        model.FormUuid = form?.uuid;
        model.ApprovalLevelsConfigured = configured;

        if (configured <= 0)
        {
            model.Title = "Approvals aren’t configured for this form";
            model.Message = "This form does not have approval levels configured, so staged approvals can’t be performed.";
            model.Hint = "Contact an administrator to set an approval level count for this form.";
            return model;
        }

        int activeStage = GetActiveStage(data, configured);
        model.ActiveApprovalLevel = activeStage <= configured ? activeStage : (int?)null;
        model.IsFullyApproved = activeStage > configured;

        model.Steps = BuildApprovalSteps(data, configured);

        var (title, message, hint) = BuildApprovalBlockedExplanation(
            data,
            attemptedLevel,
            userLevels,
            activeStage,
            configured);

        model.Title = title;
        model.Message = message;
        model.Hint = hint;

        return model;
    }

    private async Task<List<int>> GetUserApprovalLevelsAsync()
    {
        var levels = new List<int>();

        if (User?.Identity?.IsAuthenticated != true)
            return levels;

        if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L1)).Succeeded) levels.Add(1);
        if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L2)).Succeeded) levels.Add(2);
        if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L3)).Succeeded) levels.Add(3);
        if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L4)).Succeeded) levels.Add(4);
        if ((await _authorization.AuthorizeAsync(User, Permissions.Approvals_L5)).Succeeded) levels.Add(5);

        return levels;
    }

    private static (string title, string message, string? hint) BuildApprovalBlockedExplanation(
        SW_formTableDatum data,
        int? attemptedLevel,
        List<int> userLevels,
        int activeStage,
        int configuredLevels)
    {
        if (activeStage > configuredLevels)
        {
            return (
                "This item has already been fully approved",
                "There are no remaining approval steps for this item. If you expected to approve it, refresh your approvals list.",
                "Return to the approvals queue and refresh the page."
            );
        }

        int? userPrimaryLevel = userLevels.Count == 1 ? userLevels[0] : null;

        if (attemptedLevel.HasValue && attemptedLevel.Value < activeStage)
        {
            return (
                "This item has already moved forward",
                $"This approval item is now at Level {activeStage}. The link you used was for Level {attemptedLevel.Value}, which is no longer current.",
                "Go back to the approvals queue and open the item again."
            );
        }

        // Classic case: user tries to open their level, but item hasn't reached it yet
        if (userPrimaryLevel.HasValue && activeStage < userPrimaryLevel.Value)
        {
            var missing = Enumerable.Range(1, userPrimaryLevel.Value - 1)
                .Where(l => GetApprovalFlag(data, l) != 1)
                .ToList();

            var missingText = missing.Count switch
            {
                0 => "earlier approvals",
                1 => $"Approval Level {missing[0]}",
                _ => "Approval Levels " + string.Join(", ", missing)
            };

            return (
                "It’s not your turn to approve this item yet",
                $"This item is currently waiting for {missingText} to be completed before it reaches your level (Level {userPrimaryLevel.Value}).",
                "Open items only from your approval-level dropdown to avoid landing on the wrong stage."
            );
        }

        // If attempted level is later than active stage, tell them what's pending (based on active stage)
        if (attemptedLevel.HasValue && attemptedLevel.Value > activeStage)
        {
            var priorMissing = Enumerable.Range(1, Math.Max(activeStage - 1, 0))
                .Where(l => GetApprovalFlag(data, l) != 1)
                .ToList();

            var pendingText =
                priorMissing.Count > 0
                    ? "Approval Levels " + string.Join(", ", priorMissing) + $" (then Level {activeStage})"
                    : $"Approval Level {activeStage}";

            return (
                "It’s not your turn to approve this item yet",
                $"This item is currently waiting for {pendingText} to be completed.",
                "Return to the approvals queue and check back after earlier approvals are submitted."
            );
        }

        if (userLevels.Count > 0)
        {
            return (
                "This approval action isn’t available for you at this stage",
                $"Your approval access level(s): {string.Join(", ", userLevels)}. This item is currently at Level {activeStage}.",
                "If you believe you should have access here, contact an administrator."
            );
        }

        return (
            "Access denied",
            "You don’t have permission to perform this approval action at this stage.",
            "Return to the approvals queue and open the item again."
        );
    }

    private static int GetActiveStage(SW_formTableDatum data, int configuredLevels)
    {
        for (var lvl = 1; lvl <= configuredLevels; lvl++)
        {
            if (GetApprovalFlag(data, lvl) != 1)
                return lvl;
        }
        return configuredLevels + 1;
    }

    private static List<ApprovalStepStatus> BuildApprovalSteps(SW_formTableDatum data, int configuredLevels)
    {
        var steps = new List<ApprovalStepStatus>();

        for (var lvl = 1; lvl <= configuredLevels; lvl++)
        {
            steps.Add(new ApprovalStepStatus
            {
                Level = lvl,
                IsComplete = GetApprovalFlag(data, lvl) == 1,
                Approver = GetApprover(data, lvl),
                Comment = GetApprovalComment(data, lvl),
                ApprovedAt = GetApprovalDateTime(data, lvl)
            });
        }

        return steps;
    }

    // IMPORTANT: isApproval_XX are int? in the model => coalesce to 0
    private static int GetApprovalFlag(SW_formTableDatum data, int level) => level switch
    {
        1 => data.isApproval_01 ?? 0,
        2 => data.isApproval_02 ?? 0,
        3 => data.isApproval_03 ?? 0,
        4 => data.isApproval_04 ?? 0,
        5 => data.isApproval_05 ?? 0,
        _ => 0
    };

    // IMPORTANT: actual columns are isAppComment_XX (NOT isApp_comment_XX)
    private static string? GetApprovalComment(SW_formTableDatum data, int level) => level switch
    {
        1 => data.isAppComment_01,
        2 => data.isAppComment_02,
        3 => data.isAppComment_03,
        4 => data.isAppComment_04,
        5 => data.isAppComment_05,
        _ => null
    };

    private static string? GetApprover(SW_formTableDatum data, int level) => level switch
    {
        1 => data.isApprover_01,
        2 => data.isApprover_02,
        3 => data.isApprover_03,
        4 => data.isApprover_04,
        5 => data.isApprover_05,
        _ => null
    };

    private static DateTime? GetApprovalDateTime(SW_formTableDatum data, int level) => level switch
    {
        1 => data.isApp_dateTime_01,
        2 => data.isApp_dateTime_02,
        3 => data.isApp_dateTime_03,
        4 => data.isApp_dateTime_04,
        5 => data.isApp_dateTime_05,
        _ => null
    };

    private static int? TryGetInt(Dictionary<string, StringValues> q, string key)
    {
        if (!q.TryGetValue(key, out var vals)) return null;

        var s = vals.FirstOrDefault();
        return int.TryParse(s, out var v) ? v : null;
    }

    private static string? TryGetString(Dictionary<string, StringValues> q, string key)
    {
        if (!q.TryGetValue(key, out var vals)) return null;

        var s = vals.FirstOrDefault();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
