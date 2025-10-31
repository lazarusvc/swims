using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Reports;


namespace SWIMS.Data.Reports
{
    public class SwimsReportsDbContext : DbContext
    {
        public SwimsReportsDbContext(DbContextOptions<SwimsReportsDbContext> options) : base(options) { }


        public virtual DbSet<SwReport> SwReports => Set<SwReport>();
        public virtual DbSet<SwReportParam> SwReportParams => Set<SwReportParam>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SwReport>(b =>
            {
                b.ToTable("SW_reports", "rpt");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).IsRequired().HasMaxLength(256);
                b.Property(x => x.Desc).HasMaxLength(512);
                b.Property(x => x.PathOverride).HasMaxLength(256);
                b.Property(x => x.RoleId).IsRequired();
                b.Property(x => x.ParamCheck).HasDefaultValue(false);
            });


            modelBuilder.Entity<SwReportParam>(b =>
            {
                b.ToTable("SW_reports_params", "rpt");
                b.HasKey(x => x.Id);
                b.Property(x => x.ParamKey).IsRequired().HasMaxLength(128);
                b.Property(x => x.ParamValue).IsRequired().HasMaxLength(1024);
                b.Property(x => x.ParamDataType).HasMaxLength(32);
                b.HasOne(x => x.SwReport)
                .WithMany(r => r.Params)
                .HasForeignKey(x => x.SwReportId)
                .OnDelete(DeleteBehavior.Cascade);
                
            });
        }
    }
}