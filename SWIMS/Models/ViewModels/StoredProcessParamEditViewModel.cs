using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.ViewModels;

public class StoredProcessParamEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public int StoredProcessId { get; set; }

    [Required, MaxLength(128)]
    public string Key { get; set; } = string.Empty; // include '@'

    [Required]
    public string DataType { get; set; } = "NVarChar"; // must be in StoredProcDataTypes.Allowed

    public string? Value { get; set; }
}
