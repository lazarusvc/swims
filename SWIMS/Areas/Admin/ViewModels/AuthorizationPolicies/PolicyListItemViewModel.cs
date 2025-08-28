namespace SWIMS.Areas.Admin.ViewModels.AuthorizationPolicies
{
    public class PolicyListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSystem { get; set; }
        public List<string> RoleNames { get; set; } = new();
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
