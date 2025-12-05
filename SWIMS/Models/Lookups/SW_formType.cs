// File: Models/Lookups/SW_formType.cs
// Purpose: Lookup table for form types (Application, Intake, Assessment, etc.)

namespace SWIMS.Models;

public partial class SW_formType
{
    public int Id { get; set; }

    /// <summary>
    /// Stable short code, e.g. "APPLICATION", "INTAKE", "ASSESSMENT".
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// Human-friendly label, e.g. "Application", "Intake", "Assessment".
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Whether this type is currently in use.
    /// </summary>
    public bool is_active { get; set; } = true;

    /// <summary>
    /// Optional sort order for dropdowns and filters.
    /// </summary>
    public int sort_order { get; set; }
}
