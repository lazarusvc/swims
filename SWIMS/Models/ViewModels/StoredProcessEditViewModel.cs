using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.ViewModels;

public class StoredProcessEditViewModel
{
    public int? Id { get; set; }

    [Required, MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Description { get; set; }

    // Option A: use named ConnectionStrings key
    [MaxLength(128)]
    public string? ConnectionKey { get; set; }

    // Option B: explicit server + database
    [MaxLength(256)]
    public string? DataSource { get; set; }
    [MaxLength(256)]
    public string? Database { get; set; }

    // Plaintext inputs; encrypted on save if provided
    public string? DbUser { get; set; }
    [DataType(DataType.Password)]
    public string? DbPassword { get; set; }

    public string ConnectionSummary =>
        !string.IsNullOrWhiteSpace(ConnectionKey) ? $"Connection: {ConnectionKey}" : $"{DataSource}/{Database}";
}
