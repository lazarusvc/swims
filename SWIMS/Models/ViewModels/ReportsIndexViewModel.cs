using SWIMS.Models.Reports;

namespace SWIMS.Models.ViewModels
{
    public class ReportsIndexViewModel
    {
        public IEnumerable<SwReport> Reports { get; set; } = System.Linq.Enumerable.Empty<SwReport>();
        public int? SelectedId { get; set; }
        public string? ViewerUrl { get; set; }   // SSRS iframe URL or Inline action URL
        public string? ViewerMode { get; set; }  // "Ssrs" | "Inline"
        public string? Format { get; set; }      // e.g., "PDF"
    }
}
