using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SWIMS.Models;

namespace SWIMS.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Handles confirmation of a requested email address change for a user.
    /// This endpoint is typically reached via a link sent to the new email address.
    /// </summary>
    [AllowAnonymous] // ensure the emailed link works without requiring a session
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<SwUser> _userManager;
        private readonly SignInManager<SwUser> _signInManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmEmailChangeModel"/> class.
        /// </summary>
        /// <param name="userManager">The ASP.NET Core Identity <see cref="UserManager{TUser}"/> for <see cref="SwUser"/>.</param>
        /// <param name="signInManager">The ASP.NET Core Identity <see cref="SignInManager{TUser}"/> for <see cref="SwUser"/>.</param>
        public ConfirmEmailChangeModel(UserManager<SwUser> userManager, SignInManager<SwUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Gets or sets a one-time status message to display on the page (stored in TempData).
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Confirms the pending email change for the specified user, using a security token
        /// that was sent to the new email address.
        ///
        /// This method:
        /// <list type="number">
        ///   <item><description>Validates input parameters.</description></item>
        ///   <item><description>Locates the user by <paramref name="userId"/>.</description></item>
        ///   <item><description>Decodes and validates the confirmation <paramref name="code"/>.</description></item>
        ///   <item><description>Applies the email change (which also confirms the new email).</description></item>
        ///   <item><description>(Optional) Keeps username in sync with email if your app uses email-as-username.</description></item>
        ///   <item><description>Refreshes the sign-in and shows a success message.</description></item>
        /// </list>
        /// </summary>
        /// <param name="userId">The ID of the user whose email is being changed.</param>
        /// <param name="email">The new email address to confirm and set for the user.</param>
        /// <param name="code">The Base64Url-encoded confirmation token generated for the change.</param>
        /// <returns>
        /// A rendered <see cref="PageResult"/> with a status message indicating success or failure.
        /// </returns>
        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            // Basic param validation
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code))
            {
                StatusMessage = "Invalid confirmation link.";
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                StatusMessage = "We couldn’t find your account.";
                return Page();
            }

            // Decode the token
            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch
            {
                StatusMessage = "Invalid or malformed confirmation code.";
                return Page();
            }

            // Apply the email change (this also confirms the new email)
            var changeResult = await _userManager.ChangeEmailAsync(user, email, decodedToken);
            if (!changeResult.Succeeded)
            {
                StatusMessage = "This link may have expired or was already used.";
                return Page();
            }

            // If your app treats username == email, keep them aligned
            var setUserName = await _userManager.SetUserNameAsync(user, email);
            if (!setUserName.Succeeded)
            {
                StatusMessage = "Email updated, but we couldn’t update your username.";
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Thank you—your email address has been updated.";
            return Page(); // or RedirectToPage("/Account/Manage/Email", new { area = "Identity" });
        }
    }
}
