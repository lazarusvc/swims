using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.Security
{
    public class AuthorizationPolicyEntity
    {
        public int Id { get; set; }
        [MaxLength(128)] public string Name { get; set; } = default!;
        [MaxLength(512)] public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<AuthorizationPolicyRole> Roles { get; set; } = new List<AuthorizationPolicyRole>();
        public ICollection<AuthorizationPolicyClaim> Claims { get; set; } = new List<AuthorizationPolicyClaim>();
    }

    public class AuthorizationPolicyRole
    {
        public int Id { get; set; }
        public int AuthorizationPolicyEntityId { get; set; }
        public AuthorizationPolicyEntity Policy { get; set; } = default!;

        public int RoleId { get; set; }
        public SWIMS.Models.SwRole Role { get; set; } = default!;

        [MaxLength(256)] public string RoleName { get; set; } = default!; // denormalized for speed
    }

    public class AuthorizationPolicyClaim
    {
        public int Id { get; set; }
        public int AuthorizationPolicyEntityId { get; set; }
        public AuthorizationPolicyEntity Policy { get; set; } = default!;
        [MaxLength(256)] public string Type { get; set; } = default!;
        [MaxLength(256)] public string? Value { get; set; }
    }
}
