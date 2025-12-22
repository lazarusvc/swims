namespace SWIMS.Models.Lookups;

public partial class SW_formProgramTag
{
    public int SW_formsId { get; set; }
    public int SW_programTagId { get; set; }

    public SW_programTag? SW_programTag { get; set; }
}
