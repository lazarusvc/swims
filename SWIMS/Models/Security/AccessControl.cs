using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.Security
{
    public static class MatchTypes
    {
        public const string ControllerAction = "ControllerAction";
        public const string Controller = "Controller";
        public const string RazorPage = "RazorPage";
        public const string Path = "Path";
        public const string Regex = "Regex";

        public static readonly string[] All = [ControllerAction, Controller, RazorPage, Path, Regex];
    }

    public class PublicEndpoint
    {
        public int Id { get; set; }

        [MaxLength(32)] public string MatchType { get; set; } = MatchTypes.ControllerAction;
        [MaxLength(64)] public string? Area { get; set; }       // nullable = root area
        [MaxLength(128)] public string? Controller { get; set; }
        [MaxLength(128)] public string? Action { get; set; }
        [MaxLength(256)] public string? Page { get; set; }       // Razor Pages route, e.g. "/Privacy"
        [MaxLength(512)] public string? Path { get; set; }       // request path match
        [MaxLength(512)] public string? Regex { get; set; }      // regex for path
        [MaxLength(512)] public string? Notes { get; set; }

        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class EndpointPolicyAssignment
    {
        public int Id { get; set; }

        [MaxLength(32)] public string MatchType { get; set; } = MatchTypes.ControllerAction;
        [MaxLength(64)] public string? Area { get; set; }
        [MaxLength(128)] public string? Controller { get; set; }
        [MaxLength(128)] public string? Action { get; set; }
        [MaxLength(256)] public string? Page { get; set; }
        [MaxLength(512)] public string? Path { get; set; }
        [MaxLength(512)] public string? Regex { get; set; }
        [MaxLength(512)] public string? Notes { get; set; }

        // Policy link
        public int PolicyId { get; set; }
        public AuthorizationPolicyEntity Policy { get; set; } = default!;

        [MaxLength(128)] public string PolicyName { get; set; } = default!; // denormalized for speed

        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
