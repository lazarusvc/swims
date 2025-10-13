namespace SWIMS.Models.Messaging;

public sealed class ConversationMember
{
    public Guid ConversationId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedUtc { get; set; }

    public Guid? LastReadMessageId { get; set; }
    public DateTime? LastReadUtc { get; set; }

    public Conversation Conversation { get; set; } = default!;
}
