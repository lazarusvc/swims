using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Notifications;

namespace SWIMS.Data
{
    public partial class SwimsIdentityDbContext
    {
        public virtual DbSet<UserPushSubscription> PushSubscriptions { get; set; } = default!;

        private void MapPush(ModelBuilder b)
        {
            b.Entity<UserPushSubscription>(e =>
            {
                e.ToTable("push_subscriptions", schema: "notify");
                e.HasKey(x => x.Id);

                e.Property(x => x.Endpoint).HasMaxLength(1000).IsRequired();
                e.Property(x => x.P256dh).HasMaxLength(256).IsRequired();
                e.Property(x => x.Auth).HasMaxLength(256).IsRequired();
                e.Property(x => x.UserAgent).HasMaxLength(512);

                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.Endpoint).IsUnique();
            });
        }
    }
}
