using Microsoft.EntityFrameworkCore;
using SWIMS.Models;

namespace SWIMS.Data.Cases
{
    public class SwimsCasesDbContext : DbContext
    {
        public SwimsCasesDbContext(DbContextOptions<SwimsCasesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<SW_case> SW_cases => Set<SW_case>();
        public virtual DbSet<SW_caseForm> SW_caseForms => Set<SW_caseForm>();
        public virtual DbSet<SW_caseAssignment> SW_caseAssignments => Set<SW_caseAssignment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Keep all case tables in their own schema: case.*
            modelBuilder.HasDefaultSchema("case");

            // ------------------------------------------------
            // SW_case
            // ------------------------------------------------
            modelBuilder.Entity<SW_case>(b =>
            {
                b.ToTable("SW_case"); // case.SW_case
                b.HasKey(x => x.Id);

                b.Property(x => x.case_number)
                    .IsRequired()
                    .HasMaxLength(64);

                b.Property(x => x.SW_beneficiaryId)
                    .IsRequired();

                b.Property(x => x.title)
                    .IsRequired()
                    .HasMaxLength(256);

                b.Property(x => x.status)
                    .IsRequired()
                    .HasMaxLength(64);

                b.Property(x => x.program_tag)
                    .HasMaxLength(128);

                b.Property(x => x.created_at)
                    .HasColumnType("datetime2");

                b.Property(x => x.created_by)
                    .HasMaxLength(256);

                b.Property(x => x.closed_at)
                    .HasColumnType("datetime2");

                b.Property(x => x.notes)
                    .HasColumnType("nvarchar(max)");

                // IMPORTANT: we don't let this context manage the SW_beneficiary table.
                b.Ignore(x => x.SW_beneficiary);
            });

            // ------------------------------------------------
            // SW_caseForm
            // ------------------------------------------------
            modelBuilder.Entity<SW_caseForm>(b =>
            {
                b.ToTable("SW_caseForm"); // case.SW_caseForm
                b.HasKey(x => x.Id);

                b.Property(x => x.SW_caseId).IsRequired();
                b.Property(x => x.SW_formTableDatumId).IsRequired();

                b.Property(x => x.form_role)
                    .HasMaxLength(64);

                b.Property(x => x.is_primary_application)
                    .HasDefaultValue(false);

                b.Property(x => x.linked_at)
                    .HasColumnType("datetime2");

                b.Property(x => x.linked_by)
                    .HasMaxLength(256);

                b.HasOne(x => x.SW_case)
                    .WithMany(c => c.SW_caseForms)
                    .HasForeignKey(x => x.SW_caseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Again, don't touch the core forms table from this context.
                b.Ignore(x => x.SW_formTableDatum);
            });

            // ------------------------------------------------
            // SW_caseAssignment
            // ------------------------------------------------
            modelBuilder.Entity<SW_caseAssignment>(b =>
            {
                b.ToTable("SW_caseAssignment"); // case.SW_caseAssignment
                b.HasKey(x => x.Id);

                b.Property(x => x.SW_caseId).IsRequired();

                b.Property(x => x.user_id)
                    .IsRequired()
                    .HasMaxLength(450);

                b.Property(x => x.role_on_case)
                    .HasMaxLength(64);

                b.Property(x => x.assigned_at)
                    .HasColumnType("datetime2");

                b.Property(x => x.unassigned_at)
                    .HasColumnType("datetime2");

                b.Property(x => x.is_active)
                    .HasDefaultValue(true);

                b.HasOne(x => x.SW_case)
                    .WithMany(c => c.SW_caseAssignments)
                    .HasForeignKey(x => x.SW_caseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
