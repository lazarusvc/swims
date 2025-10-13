using Microsoft.EntityFrameworkCore;
using SWIMS.Models.Messaging;

namespace SWIMS.Data;

public partial class SwimsIdentityDbContext
{
    public DbSet<Conversation> Conversations { get; set; } = default!;
    public DbSet<ConversationMember> ConversationMembers { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;


    private static void MapMessaging(ModelBuilder modelBuilder)
    {
        // schema: msg
        modelBuilder.Entity<Conversation>(b =>
        {
            b.ToTable("conversations", "msg");
            b.HasKey(x => x.Id);
            b.Property(x => x.CreatedUtc).HasColumnType("datetime2");
            b.HasMany(x => x.Members).WithOne(m => m.Conversation).HasForeignKey(m => m.ConversationId);
            b.HasMany(x => x.Messages).WithOne(m => m.Conversation).HasForeignKey(m => m.ConversationId);

            // Unique pair for Direct (UserAId < UserBId)
            b.HasIndex(x => new { x.Type, x.UserAId, x.UserBId })
             .IsUnique()
             .HasFilter("([Type] = 1)"); // Direct only
        });

        modelBuilder.Entity<ConversationMember>(b =>
        {
            b.ToTable("conversation_members", "msg");
            b.HasKey(x => new { x.ConversationId, x.UserId });
            b.Property(x => x.JoinedUtc).HasColumnType("datetime2");
            b.Property(x => x.LastReadUtc).HasColumnType("datetime2");
            b.HasIndex(x => new { x.UserId, x.ConversationId });
        });

        modelBuilder.Entity<Message>(b =>
        {
            b.ToTable("messages", "msg");
            b.HasKey(x => x.Id);
            b.Property(x => x.CreatedUtc).HasColumnType("datetime2");
            b.HasIndex(x => new { x.ConversationId, x.CreatedUtc });
            b.Property(x => x.Body).HasMaxLength(4000);
        });
    }
}
