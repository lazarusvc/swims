namespace SWIMS.Models.ViewModels;

public class RunParamViewModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DataType { get; set; } = "NVarChar";
    public string? Value { get; set; }
}
