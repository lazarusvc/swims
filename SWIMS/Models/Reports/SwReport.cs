using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SWIMS.Models.Reports
{
    public class SwReport
    {
        public int Id { get; set; }


        [Required, MaxLength(256)]
        public string Name { get; set; } = default!; // e.g. "Monthly Report.rdl" or logical alias


        [MaxLength(512)]
        public string? Desc { get; set; }


        [MaxLength(256)]
        public string? PathOverride { get; set; } // optional SSRS folder/name override


        [Required]
        public string RoleId { get; set; } = default!; // FK to AspNetRoles (SwRole)


        public bool ParamCheck { get; set; }


        public ICollection<SwReportParam> Params { get; set; } = new List<SwReportParam>();
    }
}