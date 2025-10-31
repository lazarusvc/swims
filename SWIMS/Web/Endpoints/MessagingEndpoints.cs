using Microsoft.AspNetCore.Routing;      // ← added for LinkGenerator
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data;
using SWIMS.Models.Messaging;
using SWIMS.Services.Diagnostics.Auditing;
using SWIMS.Services.Messaging;
using SWIMS.Services.Notifications;
using SWIMS.Web.Hubs;
using System.Security.Claims;

namespace SWIMS.Web.Endpoints;

public static class MessagingEndpoints
{
    public static IEndpointRouteBuilder MapSwimsMessagingEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("me/chats").RequireAuthorization();

        // ---- helpers --------------------------------------------------------
        static int CurrentUserId(ClaimsPrincipal u)
            => int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);

        static string Norm(string s) => s.Trim().ToUpperInvariant();

        // ---- start by numeric userId (dev-friendly) ------------------------
        grp.MapPost("start", async (HttpContext http, SwimsIdentityDbContext db, IAuditLogger audit, int userId) =>
        {
            var me = CurrentUserId(http.User);
            if (userId == me) return Results.BadRequest(new { error = "cannot chat with yourself" });

            var a = Math.Min(me, userId);
            var b = Math.Max(me, userId);

            var convo = await db.Conversations
                .FirstOrDefaultAsync(x => x.Type == ConversationType.Direct && x.UserAId == a && x.UserBId == b);

            if (convo is null)
            {
                convo = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Type = ConversationType.Direct,
                    CreatedUtc = DateTime.UtcNow,
                    CreatedByUserId = me,
                    UserAId = a,
                    UserBId = b
                };
                db.Conversations.Add(convo);
                db.ConversationMembers.AddRange(
                    new ConversationMember { ConversationId = convo.Id, UserId = a, JoinedUtc = DateTime.UtcNow },
                    new ConversationMember { ConversationId = convo.Id, UserId = b, JoinedUtc = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();

                await audit.LogAsync("ConversationCreated", "Conversation", convo.Id.ToString(), me, http.User.Identity?.Name ?? "unknown");
            }

            return Results.Ok(new { id = convo.Id });
        });

        // ---- start by USERNAME ---------------------------------------------
        grp.MapPost("start/username", async (HttpContext http, SwimsIdentityDbContext db, IAuditLogger audit, string username) =>
        {
            var me = CurrentUserId(http.User);
            if (string.IsNullOrWhiteSpace(username)) return Results.BadRequest(new { error = "username required" });

            var norm = Norm(username);
            var otherId = await db.Users
                .Where(u => u.NormalizedUserName == norm)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (otherId == 0) return Results.NotFound(new { error = "user not found" });
            if (otherId == me) return Results.BadRequest(new { error = "cannot chat with yourself" });

            var a = Math.Min(me, otherId);
            var b = Math.Max(me, otherId);

            var convo = await db.Conversations
                .FirstOrDefaultAsync(x => x.Type == ConversationType.Direct && x.UserAId == a && x.UserBId == b);

            if (convo is null)
            {
                convo = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Type = ConversationType.Direct,
                    CreatedUtc = DateTime.UtcNow,
                    CreatedByUserId = me,
                    UserAId = a,
                    UserBId = b
                };
                db.Conversations.Add(convo);
                db.ConversationMembers.AddRange(
                    new ConversationMember { ConversationId = convo.Id, UserId = a, JoinedUtc = DateTime.UtcNow },
                    new ConversationMember { ConversationId = convo.Id, UserId = b, JoinedUtc = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
                await audit.LogAsync("ConversationCreated", "Conversation", convo.Id.ToString(), me, http.User.Identity?.Name ?? "unknown");
            }

            return Results.Ok(new { id = convo.Id });
        });

        // ---- start by EMAIL -------------------------------------------------
        grp.MapPost("start/email", async (HttpContext http, SwimsIdentityDbContext db, IAuditLogger audit, string email) =>
        {
            var me = CurrentUserId(http.User);
            if (string.IsNullOrWhiteSpace(email)) return Results.BadRequest(new { error = "email required" });

            var norm = Norm(email);
            var otherId = await db.Users
                .Where(u => u.NormalizedEmail == norm)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (otherId == 0) return Results.NotFound(new { error = "user not found" });
            if (otherId == me) return Results.BadRequest(new { error = "cannot chat with yourself" });

            var a = Math.Min(me, otherId);
            var b = Math.Max(me, otherId);

            var convo = await db.Conversations
                .FirstOrDefaultAsync(x => x.Type == ConversationType.Direct && x.UserAId == a && x.UserBId == b);

            if (convo is null)
            {
                convo = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Type = ConversationType.Direct,
                    CreatedUtc = DateTime.UtcNow,
                    CreatedByUserId = me,
                    UserAId = a,
                    UserBId = b
                };
                db.Conversations.Add(convo);
                db.ConversationMembers.AddRange(
                    new ConversationMember { ConversationId = convo.Id, UserId = a, JoinedUtc = DateTime.UtcNow },
                    new ConversationMember { ConversationId = convo.Id, UserId = b, JoinedUtc = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
                await audit.LogAsync("ConversationCreated", "Conversation", convo.Id.ToString(), me, http.User.Identity?.Name ?? "unknown");
            }

            return Results.Ok(new { id = convo.Id });
        });

        // ---- start by LOGIN (username OR email) ----------------------------
        grp.MapPost("start/login", async (HttpContext http, SwimsIdentityDbContext db, IAuditLogger audit, string login) =>
        {
            var me = CurrentUserId(http.User);
            if (string.IsNullOrWhiteSpace(login)) return Results.BadRequest(new { error = "login (username/email) required" });

            var norm = Norm(login);
            var otherId = await db.Users
                .Where(u => u.NormalizedUserName == norm || u.NormalizedEmail == norm)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (otherId == 0) return Results.NotFound(new { error = "user not found" });
            if (otherId == me) return Results.BadRequest(new { error = "cannot chat with yourself" });

            var a = Math.Min(me, otherId);
            var b = Math.Max(me, otherId);

            var convo = await db.Conversations
                .FirstOrDefaultAsync(x => x.Type == ConversationType.Direct && x.UserAId == a && x.UserBId == b);

            if (convo is null)
            {
                convo = new Conversation
                {
                    Id = Guid.NewGuid(),
                    Type = ConversationType.Direct,
                    CreatedUtc = DateTime.UtcNow,
                    CreatedByUserId = me,
                    UserAId = a,
                    UserBId = b
                };
                db.Conversations.Add(convo);
                db.ConversationMembers.AddRange(
                    new ConversationMember { ConversationId = convo.Id, UserId = a, JoinedUtc = DateTime.UtcNow },
                    new ConversationMember { ConversationId = convo.Id, UserId = b, JoinedUtc = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
                await audit.LogAsync("ConversationCreated", "Conversation", convo.Id.ToString(), me, http.User.Identity?.Name ?? "unknown");
            }

            return Results.Ok(new { id = convo.Id });
        });

        // ---- search users (typeahead) --------------------------------------
        grp.MapGet("users/search", async (SwimsIdentityDbContext db, string q, int take = 10) =>
        {
            if (string.IsNullOrWhiteSpace(q)) return Results.Ok(new { items = Array.Empty<object>() });
            take = Math.Clamp(take, 1, 25);
            var term = q.Trim();
            var termNorm = Norm(term);

            var items = await db.Users.AsNoTracking()
                .Where(u =>
                    (u.UserName != null && u.UserName.Contains(term)) ||
                    (u.Email != null && u.Email.Contains(term)) ||
                    (u.NormalizedUserName != null && u.NormalizedUserName == termNorm) ||
                    (u.NormalizedEmail != null && u.NormalizedEmail == termNorm))
                .OrderBy(u => u.UserName)
                .Take(take)
                .Select(u => new { id = u.Id, username = u.UserName, email = u.Email })
                .ToListAsync();

            return Results.Ok(new { items });
        });

        // ---- inbox list -----------------------------------------------------
        grp.MapGet("", async (HttpContext http, SwimsIdentityDbContext db, int skip = 0, int take = 20) =>
        {
            var me = CurrentUserId(http.User);
            take = Math.Clamp(take, 1, 100);

            var q = from mine in db.ConversationMembers.AsNoTracking()
                    where mine.UserId == me
                    join c in db.Conversations.AsNoTracking() on mine.ConversationId equals c.Id
                    join om in db.ConversationMembers.AsNoTracking() on c.Id equals om.ConversationId
                    where om.UserId != me
                    join u in db.Users.AsNoTracking() on om.UserId equals u.Id
                    select new { c, mine, otherUser = u };

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => db.Messages.Where(mm => mm.ConversationId == x.c.Id)
                                                   .Max(mm => (DateTime?)mm.CreatedUtc))
                .ThenByDescending(x => x.c.CreatedUtc)
                .Skip(skip).Take(take)
                .Select(x => new
                {
                    id = x.c.Id,
                    type = x.c.Type,
                    other = new
                    {
                        userId = x.otherUser.Id,
                        username = x.otherUser.UserName,
                        email = x.otherUser.Email,
                        firstName = EF.Property<string>(x.otherUser, "FirstName"),
                        lastName = EF.Property<string>(x.otherUser, "LastName"),
                        displayName =
                            ((EF.Property<string>(x.otherUser, "FirstName") ?? "")
                             + (EF.Property<string>(x.otherUser, "FirstName") != null
                                && EF.Property<string>(x.otherUser, "LastName") != null ? " " : "")
                             + (EF.Property<string>(x.otherUser, "LastName")
                                ?? (x.otherUser.UserName ?? x.otherUser.Email)))
                    },
                    lastMessage = db.Messages
                        .Where(mm => mm.ConversationId == x.c.Id)
                        .OrderByDescending(mm => mm.CreatedUtc)
                        .Select(mm => new { mm.Id, mm.SenderUserId, mm.Body, mm.CreatedUtc })
                        .FirstOrDefault(),
                    unread = db.Messages
                        .Where(mm => mm.ConversationId == x.c.Id && mm.SenderUserId != me &&
                                     (x.mine.LastReadUtc == null || mm.CreatedUtc > x.mine.LastReadUtc))
                        .Count()
                })
                .ToListAsync();

            return Results.Ok(new { total, skip, take, items });
        });

        // ---- thread (paged by before/after messageId) ----------------------
        grp.MapGet("{id:guid}/messages", async (HttpContext http, SwimsIdentityDbContext db, Guid id, Guid? before, Guid? after, int take = 50) =>
        {
            var me = CurrentUserId(http.User);
            take = Math.Clamp(take, 1, 200);

            var member = await db.ConversationMembers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ConversationId == id && x.UserId == me);
            if (member is null) return Results.Forbid();

            DateTime? cutoffBefore = null;
            DateTime? cutoffAfter = null;

            if (before.HasValue)
                cutoffBefore = await db.Messages.Where(m => m.Id == before.Value).Select(m => (DateTime?)m.CreatedUtc).FirstOrDefaultAsync();
            if (after.HasValue)
                cutoffAfter = await db.Messages.Where(m => m.Id == after.Value).Select(m => (DateTime?)m.CreatedUtc).FirstOrDefaultAsync();

            var q = db.Messages.AsNoTracking().Where(m => m.ConversationId == id);

            if (cutoffBefore.HasValue) q = q.Where(m => m.CreatedUtc < cutoffBefore.Value);
            if (cutoffAfter.HasValue) q = q.Where(m => m.CreatedUtc > cutoffAfter.Value);

            var msgs = await q.OrderByDescending(m => m.CreatedUtc)
                              .Take(take)
                              .Select(m => new { m.Id, m.SenderUserId, m.Body, m.CreatedUtc })
                              .ToListAsync();

            msgs.Reverse(); // newest-last for the UI
            return Results.Ok(new { items = msgs });
        });

        // ---- send message ---------------------------------------------------
        grp.MapPost("{id:guid}/messages", async (
            HttpContext http, SwimsIdentityDbContext db,
            IHubContext<ChatsHub> hub, IAuditLogger audit,
            IChatPresence presence, INotifier notifier,
            LinkGenerator link,                     // ← added
            Guid id, MessagePost body) =>
        {
            var me = CurrentUserId(http.User);
            var member = await db.ConversationMembers.FirstOrDefaultAsync(x => x.ConversationId == id && x.UserId == me);
            if (member is null) return Results.Forbid();

            var text = (body.Body ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(text)) return Results.BadRequest(new { error = "message cannot be empty" });
            if (text.Length > 4000) text = text[..4000];

            var msg = new Message { Id = Guid.NewGuid(), ConversationId = id, SenderUserId = me, Body = text, CreatedUtc = DateTime.UtcNow };
            db.Messages.Add(msg);
            await db.SaveChangesAsync();

            await audit.LogAsync("MessageSent", "Message", msg.Id.ToString(), me, http.User.Identity?.Name ?? "unknown",
                                 extra: new { conversationId = id });

            await hub.Clients.Group($"c:{id}").SendAsync("message", new
            {
                id = msg.Id,
                convoId = id,
                fromUserId = me,
                body = msg.Body,
                createdUtc = msg.CreatedUtc
            });

            // Notify recipient if they aren't in this convo
            var other = await (from om in db.ConversationMembers
                               where om.ConversationId == id && om.UserId != me
                               join u in db.Users on om.UserId equals u.Id
                               select new
                               {
                                   u.Id,
                                   u.UserName,
                                   u.Email,
                                   FirstName = EF.Property<string>(u, "FirstName"),
                                   LastName = EF.Property<string>(u, "LastName")
                               }).FirstAsync();

            if (!presence.IsUserInConversation(other.Id, id))
            {
                var meUser = await db.Users.AsNoTracking().Where(u => u.Id == me)
                    .Select(u => new {
                        u.UserName,
                        u.Email,
                        FirstName = EF.Property<string>(u, "FirstName"),
                        LastName = EF.Property<string>(u, "LastName")
                    }).FirstAsync();

                var fromName = (meUser.FirstName != null || meUser.LastName != null)
                    ? $"{meUser.FirstName} {meUser.LastName}".Trim()
                    : (meUser.UserName ?? meUser.Email ?? "Someone");

                var snippet = text.Length > 120 ? text[..120] + "…" : text;

                // Build absolute link like Url.Page(..., protocol: Request.Scheme)
                var path = link.GetPathByPage(http, page: "/Messenger/Chat", handler: null,
                                              values: new { area = "Portal", convoId = id });
                var chatUrl = $"{http.Request.Scheme}://{http.Request.Host}{path}";

                await notifier.NotifyUserAsync(
                    other.Id,
                    other.UserName ?? other.Email ?? $"user:{other.Id}",
                    "NewMessage",
                    new
                    {
                        messageId = msg.Id,
                        fromUserId = me,
                        fromName,
                        convoId = id,
                        snippet,
                        url = chatUrl,            // ← absolute
                        actionLabel = "Open chat" // ← short button text for email template
                    }
                );
            }

            return Results.Ok(new { id = msg.Id, msg.CreatedUtc });
        });

        // ---- mark read ------------------------------------------------------
        grp.MapPost("{id:guid}/read", async (HttpContext http, SwimsIdentityDbContext db,
                                              IHubContext<ChatsHub> hub, Guid id, MarkReadPost body) =>
        {
            var me = CurrentUserId(http.User);
            var m = await db.ConversationMembers.FirstOrDefaultAsync(x => x.ConversationId == id && x.UserId == me);
            if (m is null) return Results.Forbid();

            m.LastReadMessageId = body.LastReadMessageId;
            m.LastReadUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await hub.Clients.Group($"c:{id}").SendAsync("read", new { convoId = id, userId = me, lastReadMessageId = body.LastReadMessageId });
            return Results.Ok(new { ok = true });
        });

        return app;
    }

    // request bodies
    public record MessagePost(string Body);
    public record MarkReadPost(Guid? LastReadMessageId);
}
