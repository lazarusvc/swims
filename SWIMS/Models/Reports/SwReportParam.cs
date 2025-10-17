using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;             // + add
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;   // + add

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

        [BindNever]        // <-- do not bind from the form
        [ValidateNever]    // <-- do not validate; FK is what matters
        public SwReport SwReport { get; set; } = default!;


    }
}