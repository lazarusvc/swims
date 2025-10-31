// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SWIMS.Models;
using SWIMS.Services.Email;
using SWIMS.Models.Email;

namespace SWIMS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<SwUser> _signInManager;
        private readonly UserManager<SwUser> _userManager;
        private readonly IUserStore<SwUser> _userStore;
        private readonly IUserEmailStore<SwUser> _emailStore;
        private readonly IEmailService _emails;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<SwUser> signInManager,
            UserManager<SwUser> userManager,
            IUserStore<SwUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailService emails)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emails = emails;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            // Optional first name for nicer email templates; prefilled from external claims when available
            public string? FirstName { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // If the user already has a login, sign them in.
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.",
                    info.Principal?.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            // If the user does not have an account, ask the user to create one.
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;

            string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            string firstName = GetFirstName(info.Principal);

            Input = new InputModel
            {
                Email = email,
                FirstName = firstName
            };

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (!ModelState.IsValid)
            {
                ProviderDisplayName = info.ProviderDisplayName;
                ReturnUrl = returnUrl;
                return Page();
            }

            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            // Try to hydrate FirstName on the user if your SwUser supports it
            try
            {
                var claimFirst = GetFirstName(info.Principal);
                var chosen = !string.IsNullOrWhiteSpace(Input?.FirstName) ? Input.FirstName : claimFirst;
                // If SwUser has a FirstName property, set it (ignore if not)
                var prop = typeof(SwUser).GetProperty("FirstName");
                if (prop != null && prop.CanWrite)
                    prop.SetValue(user, string.IsNullOrWhiteSpace(chosen) ? null : chosen);
            }
            catch { /* ignore: property may not exist; safe no-op */ }

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var codeEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code = codeEncoded },
                        protocol: Request.Scheme);

                    // Send via SWIMS email templates with robust FirstName fallback
                    await _emails.SendTemplateAsync(
                        TemplateKeys.ConfirmEmail,
                        new EmailAddress(Input.Email, Input?.FirstName),
                        new
                        {
                            ConfirmationLink = callbackUrl,
                            FirstName = (string)(
                                typeof(SwUser).GetProperty("FirstName")?.GetValue(user) as string
                                ?? Input?.FirstName
                                ?? string.Empty
                            )
                        });

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private static string GetFirstName(ClaimsPrincipal principal)
        {
            // Common claim names across providers
            return principal.FindFirstValue(ClaimTypes.GivenName)
                ?? principal.FindFirstValue("given_name")
                ?? principal.FindFirstValue("givenname")
                ?? principal.FindFirstValue("first_name")
                ?? principal.FindFirstValue(ClaimTypes.Name)
                ?? principal.FindFirstValue("name");
        }

        private SwUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<SwUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(SwUser)}'. " +
                    $"Ensure that '{nameof(SwUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<SwUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");

            return (IUserEmailStore<SwUser>)_userStore;
        }
    }
}
