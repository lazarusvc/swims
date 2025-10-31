using Microsoft.Graph.Models;
using System.ComponentModel.DataAnnotations;

namespace SWIMS.Models.Messaging;

public enum ConversationType : byte { Direct = 1 }

public sealed class Conversation
{
    [Key] public Guid Id { get; set; }
    public ConversationType Type { get; set; } = ConversationType.Direct;
    public DateTime CreatedUtc { get; set; }
    public int CreatedByUserId { get; set; }

    // For Direct conversations, enforce normalized pair (A < B)
    public int? UserAId { get; set; }
    public int? UserBId { get; set; }

    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
