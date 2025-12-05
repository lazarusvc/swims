// File: Models/Lookups/SW_programTag.cs
// Purpose: Lookup table for program tags used across the system (cases, forms, reports, etc.)

namespace SWIMS.Models;

public partial class SW_programTag
{
    public int Id { get; set; }

    /// <summary>
    /// Stable short code, e.g. "PA", "SPS", "CTP".
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// Human-friendly label, e.g. "Public Assistance".
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Whether this program tag is currently in use.
    /// </summary>
    public bool is_active { get; set; } = true;

    /// <summary>
    /// Optional sort order for dropdowns and filters.
    /// </summary>
    public int sort_order { get; set; }
}
