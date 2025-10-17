using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using SWIMS.Models.Email;

// Explicit aliases to avoid name collisions with your domain EmailAddress
using GraphBodyType = Microsoft.Graph.Models.BodyType;
using GraphEmailAddress = Microsoft.Graph.Models.EmailAddress;
using GraphItemBody = Microsoft.Graph.Models.ItemBody;
using GraphMessage = Microsoft.Graph.Models.Message;
using GraphRecipient = Microsoft.Graph.Models.Recipient;
using GraphAttachment = Microsoft.Graph.Models.Attachment;
using GraphFileAttachment = Microsoft.Graph.Models.FileAttachment;

namespace SWIMS.Services.Email;

public sealed class GraphEmailService : IEmailService
{
    private readonly GraphMailConfiguration _cfg;
    private readonly ILogger<GraphEmailService> _logger;
    private readonly ITemplateRenderer _renderer;
    private readonly GraphServiceClient _graph;

    public GraphEmailService(
        IOptions<GraphMailConfiguration> cfg,
        ILogger<GraphEmailService> logger,
        ITemplateRenderer renderer)
    {
        _cfg = cfg.Value;
        _logger = logger;
        _renderer = renderer;

        // App-only auth (client credentials)
        var credential = new ClientSecretCredential(_cfg.TenantId, _cfg.ClientId, _cfg.ClientSecret);
        _graph = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
    }

    public async Task SendAsync(
        SWIMS.Models.Email.EmailAddress to, string subject, string htmlBody,
        SWIMS.Models.Email.EmailAddress? from = null,
        IEnumerable<SWIMS.Models.Email.EmailAddress>? cc = null,
        IEnumerable<SWIMS.Models.Email.EmailAddress>? bcc = null,
        IEnumerable<SWIMS.Models.Email.EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        var msg = BuildMessage(to, subject, htmlBody, from, cc, bcc, attachments);

        await _graph.Users[_cfg.SenderUser].SendMail.PostAsync(
            new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = msg,
                SaveToSentItems = _cfg.SaveToSentItems
            },
            cancellationToken: ct
        );

        _logger.LogInformation("Graph mail sent as {Sender} to {To} | Subject: {Subject}",
            _cfg.SenderUser, to.Address, subject);
    }

    public async Task SendTemplateAsync(
        string templateKey,
        SWIMS.Models.Email.EmailAddress to,
        object model,
        SWIMS.Models.Email.EmailAddress? from = null,
        IEnumerable<SWIMS.Models.Email.EmailAddress>? cc = null,
        IEnumerable<SWIMS.Models.Email.EmailAddress>? bcc = null,
        IEnumerable<SWIMS.Models.Email.EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        // Your renderer in this project returns an object/tuple with .Subject and .Html
        var rendered = await _renderer.RenderAsync(templateKey, model);
        var subject = rendered.Subject;
        var html = rendered.HtmlBody; 


        await SendAsync(to, subject, html, from, cc, bcc, attachments, ct);
    }

    private static GraphMessage BuildMessage(
    SWIMS.Models.Email.EmailAddress to, string subject, string htmlBody,
    SWIMS.Models.Email.EmailAddress? from,
    IEnumerable<SWIMS.Models.Email.EmailAddress>? cc,
    IEnumerable<SWIMS.Models.Email.EmailAddress>? bcc,
    IEnumerable<SWIMS.Models.Email.EmailAttachment>? attachments)
    {
        var msg = new GraphMessage
        {
            Subject = subject,
            Body = new GraphItemBody { ContentType = GraphBodyType.Html, Content = htmlBody },
            ToRecipients = new List<GraphRecipient> {
            new() {
                EmailAddress = new GraphEmailAddress { Address = to.Address, Name = to.DisplayName }
            }
        }
        };

        // Map helper
        static List<GraphRecipient>? Map(IEnumerable<SWIMS.Models.Email.EmailAddress>? src)
            => src?.Select(a => new GraphRecipient
            {
                EmailAddress = new GraphEmailAddress { Address = a.Address, Name = a.DisplayName }
            }).ToList();

        var ccList = Map(cc);
        if (ccList is { Count: > 0 })
            msg.CcRecipients = ccList;        // only set when there are items

        var bccList = Map(bcc);
        if (bccList is { Count: > 0 })
            msg.BccRecipients = bccList;      // only set when there are items

        if (attachments != null)
        {
            var atts = new List<GraphAttachment>();
            foreach (var att in attachments)
            {
                atts.Add(new GraphFileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    Name = att.FileName,
                    ContentType = att.ContentType,
                    ContentBytes = att.Content
                });
            }
            if (atts.Count > 0)
                msg.Attachments = atts;        // only set when there are items
        }

        // Note: 'from' is unused for app-only; we send as SenderUser
        return msg;
    }

}
