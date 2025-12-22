// File: Data/Lookups/SwimsLookupDbContext.cs

using Microsoft.EntityFrameworkCore;
using SWIMS.Models;
using SWIMS.Models.Lookups;

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

        public virtual DbSet<SW_formProgramTag> SW_formProgramTags { get; set; } = null!;
        public virtual DbSet<SW_formFormType> SW_formFormTypes { get; set; } = null!;


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


            // ----------------------------
            // SW_formProgramTag
            // ----------------------------
            modelBuilder.Entity<SW_formProgramTag>(entity =>
            {
                entity.ToTable("SW_formProgramTag", "ref");

                entity.HasKey(e => new { e.SW_formsId, e.SW_programTagId });

                entity.HasIndex(e => e.SW_formsId);
                entity.HasIndex(e => e.SW_programTagId);

                entity.HasOne(d => d.SW_programTag)
                    .WithMany()
                    .HasForeignKey(d => d.SW_programTagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ----------------------------
            // SW_formFormType
            // ----------------------------
            modelBuilder.Entity<SW_formFormType>(entity =>
            {
                entity.ToTable("SW_formFormType", "ref");

                // PK = SW_formsId => only 1 record per form
                entity.HasKey(e => e.SW_formsId);
                entity.Property(e => e.SW_formsId).ValueGeneratedNever();

                entity.HasIndex(e => e.SW_formTypeId);

                entity.HasOne(d => d.SW_formType)
                    .WithMany()
                    .HasForeignKey(d => d.SW_formTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
