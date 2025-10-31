// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SWIMS.Models;
using SWIMS.Services.Email;
using SWIMS.Models.Email;

namespace SWIMS.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<SwUser> _userManager;
        private readonly IEmailService _emails;

        public ForgotPasswordModel(UserManager<SwUser> userManager, IEmailService emails)
        {
            _userManager = userManager;
            _emails = emails;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emails.SendTemplateAsync(
                    TemplateKeys.ResetPassword,
                    new EmailAddress(user!.Email!, user.FirstName),
                    new
                    {
                        SubjectLine = "Reset your SWIMS password",
                        BodyIntro = "A request was received to reset the password for your SWIMS account. If this was you, use the button below to continue.",
                        MainParagraph = "For your protection, the reset link will expire after a short time and can be used only once. If you did not request a password reset, you may safely ignore this message and your password will remain unchanged.",

                        ShowCTA = true,
                        ActionLabel = "Reset Password",
                        ActionUrl = callbackUrl,              // formerly ResetLink

                        SupportEmail = "support.apps@gov.dm",
                        SupportPhone = "(767) 266-3310",
                        // ReferenceId = referenceId
                    });


                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
