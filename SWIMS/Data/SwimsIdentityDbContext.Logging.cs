using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Logging;

namespace SWIMS.Data
{
    public partial class SwimsIdentityDbContext
    {
        public virtual DbSet<AuditLog> AuditLogs { get; set; } = default!;

        private void MapLogging(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("audit_logs", schema: "log");
                b.HasKey(x => x.Id);
                b.Property(x => x.Action).IsRequired().HasMaxLength(16);
                b.Property(x => x.Entity).IsRequired().HasMaxLength(256);
                b.Property(x => x.EntityId).HasMaxLength(256);
                b.Property(x => x.Username).HasMaxLength(256);
                b.Property(x => x.Ip).HasMaxLength(64);
                b.Property(x => x.OldValuesJson).HasColumnType("nvarchar(max)");
                b.Property(x => x.NewValuesJson).HasColumnType("nvarchar(max)");

                b.HasIndex(x => new { x.Entity, x.EntityId, x.Utc });
                b.HasIndex(x => new { x.UserId, x.Utc });
            });
        }
    }
}
