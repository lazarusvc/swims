using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Notifications;

namespace SWIMS.Data
{
    public partial class SwimsIdentityDbContext
    {
        public virtual DbSet<Notification> Notifications { get; set; } = default!;

        private void MapNotifications(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(b =>
            {
                b.ToTable("notifications", schema: "notify");
                b.HasKey(x => x.Id);

                b.Property(x => x.Username).HasMaxLength(256);
                b.Property(x => x.Type).IsRequired().HasMaxLength(128);
                b.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");

                b.HasIndex(x => new { x.UserId, x.Seen, x.CreatedUtc });
                b.HasIndex(x => x.CreatedUtc);
            });
        }
    }
}
