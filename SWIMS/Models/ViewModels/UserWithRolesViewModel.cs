// -------------------------------------------------------------------
// File:    UserWithRolesViewModel.cs
// Author:  N/A
// Created: N/A
// Purpose: ViewModel representing a user and their assigned roles for display in admin UIs.
// Dependencies:
//   - SWIMS.Models.SwUser
// -------------------------------------------------------------------

using System.Collections.Generic;

namespace SWIMS.Models.ViewModels
{
    /// <summary>
    /// Represents a user along with their roles for display in user management views.
    /// </summary>
    public class UserWithRolesViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's login username.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of roles assigned to the user.
        /// </summary>
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
