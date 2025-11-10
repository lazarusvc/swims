// -------------------------------------------------------------------
// File:    RoleChoiceVM.cs
// Purpose: Checkbox list item + wrapper for assigning roles to a user.
// -------------------------------------------------------------------
using System.Collections.Generic;

namespace SWIMS.Models.ViewModels
{
    public class RoleChoiceVM
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public bool Selected { get; set; }
    }

    public class EditUserRolesVM
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = "";
        public List<RoleChoiceVM> Roles { get; set; } = new();
    }
}
