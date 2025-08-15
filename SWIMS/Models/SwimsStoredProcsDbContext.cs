using Microsoft.EntityFrameworkCore;
using SWIMS.Models;

namespace SWIMS.Models
{
    public class SwimsStoredProcsDbContext : DbContext
    {
        public SwimsStoredProcsDbContext(DbContextOptions<SwimsStoredProcsDbContext> options) : base(options) { }

        public DbSet<StoredProcess> StoredProcesses => Set<StoredProcess>();
        public DbSet<StoredProcessParam> StoredProcessParams => Set<StoredProcessParam>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("sp"); // keep module tables in their own schema

            modelBuilder.Entity<StoredProcess>(entity =>
            {
                entity.ToTable("stored_processes"); // sp.stored_processes
                entity.Property(e => e.Name).HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(1024);
                entity.Property(e => e.ConnectionKey).HasMaxLength(128);
                entity.Property(e => e.DataSource).HasMaxLength(256);
                entity.Property(e => e.Database).HasMaxLength(256);
                entity.Property(e => e.DbUserEncrypted).HasMaxLength(512);
                entity.Property(e => e.DbPasswordEncrypted).HasMaxLength(1024);
                entity.HasMany(e => e.Params)
                      .WithOne(p => p.StoredProcess)
                      .HasForeignKey(p => p.StoredProcessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StoredProcessParam>(entity =>
            {
                entity.ToTable("stored_process_params"); // sp.stored_process_params
                entity.Property(e => e.Key).HasMaxLength(128);
                entity.Property(e => e.DataType).HasMaxLength(64);
            });
        }
    }
}