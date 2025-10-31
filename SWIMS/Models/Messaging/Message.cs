using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.Messaging;

public sealed class Message
{
    [Key] public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public int SenderUserId { get; set; }

    [MaxLength(4000)] public string Body { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
    public DateTime? EditedUtc { get; set; }
    public bool Deleted { get; set; }

    public Conversation Conversation { get; set; } = default!;
}
