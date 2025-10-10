using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Outbox;

namespace SWIMS.Data
{
    public partial class SwimsIdentityDbContext
    {
        public virtual DbSet<EmailOutbox> EmailOutbox { get; set; } = default!;
        public virtual DbSet<EmailDeadLetter> EmailDeadLetters { get; set; } = default!;

        private void MapOperations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailOutbox>(b =>
            {
                b.ToTable("email_outbox", schema: "ops");
                b.HasKey(x => x.Id);

                b.Property(x => x.To).IsRequired().HasMaxLength(512);
                b.Property(x => x.Cc).HasMaxLength(1024);
                b.Property(x => x.Bcc).HasMaxLength(1024);
                b.Property(x => x.Subject).IsRequired().HasMaxLength(512);
                b.Property(x => x.HeadersJson).HasColumnType("nvarchar(max)");

                b.HasIndex(x => new { x.SentUtc, x.NextAttemptUtc });
                b.HasIndex(x => x.CreatedUtc);
            });

            modelBuilder.Entity<EmailDeadLetter>(b =>
            {
                b.ToTable("email_deadletter", schema: "ops");
                b.HasKey(x => x.Id);

                b.Property(x => x.To).IsRequired().HasMaxLength(512);
                b.Property(x => x.Subject).IsRequired().HasMaxLength(512);
                b.Property(x => x.HeadersJson).HasColumnType("nvarchar(max)");
                b.Property(x => x.Error).IsRequired().HasColumnType("nvarchar(max)");
            });
        }
    }
}
