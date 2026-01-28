using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Notifications;

namespace SWIMS.Data;

public partial class SwimsIdentityDbContext
{
    public DbSet<NotificationRoute> NotificationRoutes => Set<NotificationRoute>();

    private void MapNotificationRouting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationRoute>(b =>
        {
            b.ToTable("notification_routes", schema: "notify");
            b.HasKey(x => x.Id);

            // Expecting EventKey exists and is your stable identifier
            b.Property(x => x.EventKey)
                .HasMaxLength(256)
                .IsRequired();

            b.HasIndex(x => x.EventKey).IsUnique();

            // Leave the rest to conventions so we don’t mismatch your property names.
            // (Your JSON columns / flags will still map fine via conventions.)
        });
    }
}
