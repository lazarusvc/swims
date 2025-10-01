using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SWIMS.Models;
using SWIMS.Models.Email;

namespace SWIMS.Services.Email;

/// <summary>
/// Adapter so ASP.NET Identity can send through SWIMS email service.
/// Targets your existing user type: SWIMS.Models.SwUser
/// </summary>
public sealed class IdentityEmailSenderAdapter : IEmailSender<SwUser>
{
    private readonly IEmailService _emails;

    public IdentityEmailSenderAdapter(IEmailService emails) => _emails = emails;

    public Task SendConfirmationLinkAsync(SwUser user, string email, string confirmationLink)
        => _emails.SendTemplateAsync(TemplateKeys.ConfirmEmail, new EmailAddress(email), new { ConfirmationLink = confirmationLink, FirstName = user?.FirstName });

    public Task SendPasswordResetLinkAsync(SwUser user, string email, string resetLink)
        => _emails.SendTemplateAsync(TemplateKeys.ResetPassword, new EmailAddress(email), new { ResetLink = resetLink, FirstName = user?.FirstName });

    public Task SendPasswordResetCodeAsync(SwUser user, string email, string resetCode)
        => _emails.SendTemplateAsync(TemplateKeys.TwoFactorCode, new EmailAddress(email), new { Code = resetCode, FirstName = user?.FirstName });
}
