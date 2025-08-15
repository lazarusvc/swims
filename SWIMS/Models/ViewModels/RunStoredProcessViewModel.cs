namespace SWIMS.Models.ViewModels;

public class RunStoredProcessViewModel
{
    public int ProcessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ConnectionDisplay { get; set; }
    public List<RunParamViewModel> Params { get; set; } = new();
}
