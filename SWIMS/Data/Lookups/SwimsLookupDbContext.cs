// File: Data/Lookups/SwimsLookupDbContext.cs

using Microsoft.EntityFrameworkCore;
using SWIMS.Models;

namespace SWIMS.Data.Lookups
{
    /// <summary>
    /// Lightweight context for cross-cutting lookup tables
    /// (program tags, form types, etc.).
    /// 
    /// Intentionally separated so migrations from legacy core tables
    /// don't interfere with these.
    /// </summary>
    public class SwimsLookupDbContext : DbContext
    {
        public SwimsLookupDbContext(DbContextOptions<SwimsLookupDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<SW_programTag> SW_programTags => Set<SW_programTag>();
        public virtual DbSet<SW_formType> SW_formTypes => Set<SW_formType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Keep all lookup tables in their own schema, e.g. ref.*
            modelBuilder.HasDefaultSchema("ref");

            // ----------------------------
            // SW_programTag
            // ----------------------------
            modelBuilder.Entity<SW_programTag>(b =>
            {
                b.ToTable("SW_programTag");
                b.HasKey(x => x.Id);

                b.Property(x => x.code)
                    .IsRequired()
                    .HasMaxLength(64);

                b.Property(x => x.name)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.is_active)
                    .HasDefaultValue(true);

                b.Property(x => x.sort_order);

                b.HasIndex(x => x.code)
                    .IsUnique();
            });

            // ----------------------------
            // SW_formType
            // ----------------------------
            modelBuilder.Entity<SW_formType>(b =>
            {
                b.ToTable("SW_formType");
                b.HasKey(x => x.Id);

                b.Property(x => x.code)
                    .IsRequired()
                    .HasMaxLength(64);

                b.Property(x => x.name)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.is_active)
                    .HasDefaultValue(true);

                b.Property(x => x.sort_order);

                b.HasIndex(x => x.code)
                    .IsUnique();
            });
        }
    }
}
