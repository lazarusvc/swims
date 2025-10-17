namespace SWIMS.Areas.Admin.ViewModels.AccessControl
{
    public class EndpointPolicyListItemViewModel
    {
        public int Id { get; set; }
        public string MatchType { get; set; } = default!;
        public string? Area { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Page { get; set; }
        public string? Path { get; set; }
        public string? Regex { get; set; }
        public string PolicyName { get; set; } = default!;
        public string? Notes { get; set; }
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
