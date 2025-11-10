// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SWIMS.Models;
using SWIMS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SWIMS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<SwUser> _signInManager;
        private readonly UserManager<SwUser> _userManager;
        private readonly RoleManager<SwRole> _roleManager;

        private readonly ILogger<LoginModel> _logger;
        private readonly ILdapAuthService _ldapAuthService;
        private readonly IConfiguration _configuration;


        public LoginModel(SignInManager<SwUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<SwUser> userManager,
            RoleManager<SwRole> roleManager,
            ILdapAuthService ldapAuthService,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _ldapAuthService = ldapAuthService;
            _configuration = configuration;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [Display(Name = "Username or Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // 1) Try local Identity login by the string the user entered
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);


                // 2a) If password is correct but 2FA is required, redirect into the 2FA page
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage(
                    "./LoginWith2fa",
                    new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                // 2b) If that fails, try finding by email and retry with their UserName
                if (!result.Succeeded)
                {
                    var byEmail = await _userManager.FindByEmailAsync(Input.Email);
                    if (byEmail != null)
                    {
                        result = await _signInManager.PasswordSignInAsync(
                            byEmail.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                        if (result.RequiresTwoFactor)
                        {
                            return RedirectToPage(
                            "./LoginWith2fa",
                            new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                        }
                    }
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in with Identity.");
                    return LocalRedirect(returnUrl);
                }


                // 3) Fallback to LDAP authentication
                var ldapUser = await _ldapAuthService.AuthenticateAsync(Input.Email, Input.Password);

                if (ldapUser != null)
                {
                    _logger.LogInformation($"LDAP user '{ldapUser.Username}' authenticated successfully.");

                    var loginProvider = "LDAP";
                    var providerKey = ldapUser.Username;

                    // 1. Check if this user is already linked to LDAP
                    var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
                    if (user == null)
                    {
                        // 2. Check if a local user with that username exists
                        user = await _userManager.FindByNameAsync(ldapUser.Username);
                        if (user == null)
                        {
                            // 3. Create the local user record (no password required)
                            user = new SwUser
                            {
                                UserName = ldapUser.Username,

                                // Prefer the email from LDAP; fall back to UPN-style if needed
                                Email = !string.IsNullOrWhiteSpace(ldapUser.Email)
                                    ? ldapUser.Email
                                    : (ldapUser.Username.Contains("@")
                                        ? ldapUser.Username
                                        : $"{ldapUser.Username}@{_configuration["Ldap:UpnSuffix"]}"),

                                // Pull first/last name from LDAP when present
                                FirstName = ldapUser.FirstName ?? string.Empty,
                                LastName = ldapUser.LastName ?? string.Empty,

                                EmailConfirmed = true
                            };

                            var createResult = await _userManager.CreateAsync(user);
                            if (!createResult.Succeeded)
                            {
                                var errors = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                                _logger.LogError("LDAP user creation failed: " + errors);
                                ModelState.AddModelError("", $"Failed to create local LDAP user record: {errors}");
                                return Page();
                            }

                        }

                        // 4. Link the user to LDAP login
                        var linkResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(loginProvider, providerKey, "LDAP"));
                        if (!linkResult.Succeeded)
                        {
                            ModelState.AddModelError("", "Failed to link LDAP login to user.");
                            return Page();
                        }
                    }


                    // 5. (optional) Sync LDAP groups into ASP.NET roles *before* signing in
                    var syncLdapRoles = _configuration.GetValue<bool>("Ldap:EnableRoleSync", false);
                    if (syncLdapRoles && ldapUser.Groups != null)
                    {
                        foreach (var dn in ldapUser.Groups)
                        {
                            var m = Regex.Match(dn, @"CN=([^,]+)");
                            if (!m.Success) continue;
                            var groupName = m.Groups[1].Value;

                            // ensure the role exists
                            if (!await _roleManager.RoleExistsAsync(groupName))
                            {
                                await _roleManager.CreateAsync(new SwRole { Name = groupName });
                            }

                            // ensure the user is in that role
                            if (!await _userManager.IsInRoleAsync(user, groupName))
                            {
                                await _userManager.AddToRoleAsync(user, groupName);
                            }
                        }
                    }

                    // 5b. Persist useful LDAP profile claims in the DB (for audits/notifications)
                    //     We upsert: name, given_name, family_name, email, plus upn/sam/dn.
                    async Task UpsertAsync(string type, string value)
                    {
                        if (string.IsNullOrWhiteSpace(value)) return;

                        var existingClaims = await _userManager.GetClaimsAsync(user);
                        var existing = existingClaims.FirstOrDefault(c => c.Type == type);

                        if (existing == null)
                        {
                            await _userManager.AddClaimAsync(user, new Claim(type, value));
                        }
                        else if (!string.Equals(existing.Value, value, StringComparison.Ordinal))
                        {
                            await _userManager.RemoveClaimAsync(user, existing);
                            await _userManager.AddClaimAsync(user, new Claim(type, value));
                        }
                    }

                    var emailForClaims = !string.IsNullOrWhiteSpace(ldapUser.Email)
                        ? ldapUser.Email
                        : (ldapUser.Username.Contains("@")
                            ? ldapUser.Username
                            : $"{ldapUser.Username}@{_configuration["Ldap:UpnSuffix"]}");

                    // Standard claims
                    await UpsertAsync(ClaimTypes.Name, ldapUser.DisplayName ?? ldapUser.Username ?? string.Empty);
                    await UpsertAsync(ClaimTypes.GivenName, ldapUser.FirstName ?? string.Empty);
                    await UpsertAsync(ClaimTypes.Surname, ldapUser.LastName ?? string.Empty);
                    await UpsertAsync(ClaimTypes.Email, emailForClaims);

                    // Helpful custom claims
                    await UpsertAsync("upn", emailForClaims);
                    await UpsertAsync("sam", ldapUser.Username ?? string.Empty);
                    await UpsertAsync("dn", ldapUser.DistinguishedName ?? string.Empty);

                    // 6. Now sign in with an up-to-date cookie (including roles & DB claims)
                    await _signInManager.SignInAsync(user, isPersistent: Input.RememberMe);
                    _logger.LogInformation($"LDAP user '{user.UserName}' signed in via Identity.");

                    return LocalRedirect(returnUrl);
                }


                // If we reach here, both Identity and LDAP failed
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // If we got this far, redisplay form
            return Page();
        }

    }
}
