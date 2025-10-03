// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SWIMS.Helpers;
using SWIMS.Models;
using SWIMS.Services.Email;
using SWIMS.Models.Email;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SWIMS.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<SwUser> _userManager;
        private readonly SignInManager<SwUser> _signInManager;
        private readonly IEmailService _emails;

        public EmailModel(
            UserManager<SwUser> userManager,
            SignInManager<SwUser> signInManager,
            IEmailService emails)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emails = emails;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(SwUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (await _userManager.IsLdapUserAsync(user))
                return Forbid();


            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (await _userManager.IsLdapUserAsync(user))
                return Forbid();


            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Get current email from the store and normalize the proposed new value
            var currentEmail = await _userManager.GetEmailAsync(user);
            var newEmail = Input.NewEmail?.Trim();

            // Only proceed if something actually changed (case-insensitive)
            if (!string.Equals(newEmail, currentEmail, StringComparison.OrdinalIgnoreCase))
            {
                var userId = await _userManager.GetUserIdAsync(user);

                // Generate the change-email token for the *new* email
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
                var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Build the confirmation callback with the *new* email
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId, email = newEmail, code = codeEncoded },
                    protocol: Request.Scheme);

                await _emails.SendTemplateAsync(
                    TemplateKeys.ConfirmEmailChange,
                    new EmailAddress(newEmail!, user?.FirstName),
                    new
                    {
                        SubjectLine = "Confirm your new email address for SWIMS",
                        BodyIntro = "We received a request to update the email address on your SWIMS account. Please confirm the new address to complete this change.",
                        MainParagraph = "This confirmation link is time-limited and valid for one use. If you did not request this change, please ignore this email or contact the Information Systems Support Unit.",
                        ShowCTA = true,
                        ActionLabel = "Confirm New Email",
                        ActionUrl = callbackUrl,
                        SupportEmail = "support.apps@gov.dm",
                        SupportPhone = "(767) 266-3310"
                    });

                StatusMessage = "Confirmation link to change email sent. Please check the new email.";
                return RedirectToPage();
            }

            // If we got here, nothing changed (same address after trimming/casing)
            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();

        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (await _userManager.IsLdapUserAsync(user))
                return Forbid();


            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await _emails.SendTemplateAsync(
                TemplateKeys.ConfirmEmail, // use your key
                new EmailAddress(user!.Email!, user.FirstName),
                new
                {
                    SubjectLine = "Confirm your email for the Social Welfare Information Management System (SWIMS)",
                    BodyIntro = "Welcome to the Social Welfare Information Management System (SWIMS). Please confirm your email address to complete your account setup.",
                    MainParagraph = "For your security, this confirmation link is time-limited and valid for one use. If you did not initiate this request, you may safely ignore this message.",

                    ShowCTA = true,
                    ActionLabel = "Confirm Email",
                    ActionUrl = callbackUrl,           // formerly ConfirmationLink

                    SupportEmail = "support.apps@gov.dm", // set your real mailbox
                    SupportPhone = "(767) 266-3310",      // set your real phone
                    // ReferenceId = referenceId            // e.g., a short GUID/trace ID
                });


            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
