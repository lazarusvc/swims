namespace SWIMS.Models.Lookups;

public partial class SW_formFormType
{
    // Enforce 0..1 form type per form by making SW_formsId the PK
    public int SW_formsId { get; set; }
    public int SW_formTypeId { get; set; }

    public SW_formType? SW_formType { get; set; }
}
