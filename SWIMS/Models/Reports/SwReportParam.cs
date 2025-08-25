using System.ComponentModel.DataAnnotations;


namespace SWIMS.Models.Reports
{
    public class SwReportParam
    {
        public int Id { get; set; }


        [Required, MaxLength(128)]
        public string ParamKey { get; set; } = default!; // must match SSRS parameter


        [Required, MaxLength(1024)]
        public string ParamValue { get; set; } = default!;


        [MaxLength(32)]
        public string? ParamDataType { get; set; } // String, Integer, DateTime, etc.


        public int SwReportId { get; set; }
        public SwReport SwReport { get; set; } = default!;


        public int? SwSiteIdentityId { get; set; } // optional scoping per site/office
        public SwSiteIdentity? SwSiteIdentity { get; set; }
    }
}