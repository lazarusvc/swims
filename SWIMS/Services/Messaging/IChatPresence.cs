namespace SWIMS.Services.Messaging;

public interface IChatPresence
{
    Task JoinAsync(int userId, Guid convoId, string connectionId);
    Task LeaveAsync(int userId, string connectionId);
    bool IsUserInConversation(int userId, Guid convoId);
}
