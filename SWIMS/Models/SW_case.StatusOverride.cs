namespace SWIMS.Models;

public partial class SW_case
{
    public string? status_override { get; set; }
    public string? status_override_reason { get; set; }
    public DateTime? status_override_until { get; set; }
    public DateTime? status_override_at { get; set; }
    public string? status_override_by { get; set; }
}
