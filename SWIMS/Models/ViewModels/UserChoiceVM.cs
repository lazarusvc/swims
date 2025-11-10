// -------------------------------------------------------------------
// File:    UserChoiceVM.cs
// Purpose: Checkbox list item + wrapper for assigning users to a role.
// -------------------------------------------------------------------
using System.Collections.Generic;

namespace SWIMS.Models.ViewModels
{
    public class UserChoiceVM
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Selected { get; set; }
    }

    public class EditRoleUsersVM
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public List<UserChoiceVM> Users { get; set; } = new();
    }
}
