using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Security;

namespace SWIMS.Data
{
	public partial class SwimsIdentityDbContext
	{
		public virtual DbSet<AuthorizationPolicyEntity> AuthorizationPolicies { get; set; } = default!;
		public virtual DbSet<AuthorizationPolicyRole> AuthorizationPolicyRoles { get; set; } = default!;
		public virtual DbSet<AuthorizationPolicyClaim> AuthorizationPolicyClaims { get; set; } = default!;


        // >>> SINGLE implementation of OnModelCreatingPartial
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            MapAuthPolicies(modelBuilder);     // defined in THIS file
            MapAccessControl(modelBuilder);    // defined in AccessControl partial (other file)
            MapLogging(modelBuilder);
            MapNotifications(modelBuilder);
        }

        // Local mapper for the policy tables
        private void MapAuthPolicies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthorizationPolicyEntity>(b =>
			{
				b.ToTable("policies", "auth");
				b.HasKey(x => x.Id);
				b.HasIndex(x => x.Name).IsUnique();
				b.Property(x => x.Name).IsRequired().HasMaxLength(128);
				b.Property(x => x.Description).HasMaxLength(512);
				b.Property(x => x.IsEnabled).HasDefaultValue(true);
                b.Property(x => x.IsSystem).HasDefaultValue(false);
                b.Property(x => x.UpdatedAt);
			});

			modelBuilder.Entity<AuthorizationPolicyRole>(b =>
			{
				b.ToTable("policy_roles", "auth");
				b.HasKey(x => x.Id);

				b.Property(x => x.RoleName).IsRequired().HasMaxLength(256);

				b.HasOne(x => x.Policy)
					.WithMany(p => p.Roles)
					.HasForeignKey(x => x.AuthorizationPolicyEntityId)
					.OnDelete(DeleteBehavior.Cascade);

				b.HasOne(x => x.Role)
					.WithMany()
					.HasForeignKey(x => x.RoleId)
					.OnDelete(DeleteBehavior.Cascade);

				b.HasIndex(x => new { x.AuthorizationPolicyEntityId, x.RoleId }).IsUnique();
			});

			modelBuilder.Entity<AuthorizationPolicyClaim>(b =>
			{
				b.ToTable("policy_claims", "auth");
				b.HasKey(x => x.Id);

				b.HasOne(x => x.Policy)
					.WithMany(p => p.Claims)
					.HasForeignKey(x => x.AuthorizationPolicyEntityId)
					.OnDelete(DeleteBehavior.Cascade);

				b.Property(x => x.Type).IsRequired().HasMaxLength(256);
				b.Property(x => x.Value).HasMaxLength(256);
			});
		}
	}
}
