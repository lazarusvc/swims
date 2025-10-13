using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SWIMS.Areas.Portal.Pages.Messages;

[Authorize]
public class ChatModel : PageModel
{
    public int MeUserId { get; private set; }
    public int? StartUserId { get; private set; }
    public Guid? OpenConversationId { get; private set; }

    public void OnGet()
    {
        // Current user id (Identity PK) from NameIdentifier claim
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var me))
            throw new InvalidOperationException("Unable to resolve current user id.");

        MeUserId = me;

        // Optional deep-links (?userId= or ?convoId=)
        if (int.TryParse(Request.Query["userId"], out var uid)) StartUserId = uid;

        if (Guid.TryParse(Request.Query["convoId"], out var cid)) OpenConversationId = cid;
    }
}
