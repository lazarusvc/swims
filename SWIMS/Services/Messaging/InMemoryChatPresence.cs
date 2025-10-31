using System.Collections.Concurrent;

namespace SWIMS.Services.Messaging;

public sealed class InMemoryChatPresence : IChatPresence
{
    // connId -> (userId, [convoIds])
    private readonly ConcurrentDictionary<string, (int userId, ConcurrentDictionary<Guid, byte>)> _conns = new();
    // convoId -> userIds
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, byte>> _convoUsers = new();

    public Task JoinAsync(int userId, Guid convoId, string connectionId)
    {
        var tuple = _conns.GetOrAdd(connectionId, _ => (userId, new ConcurrentDictionary<Guid, byte>()));
        tuple.Item2[convoId] = 1;

        var users = _convoUsers.GetOrAdd(convoId, _ => new ConcurrentDictionary<int, byte>());
        users[userId] = 1;
        return Task.CompletedTask;
    }

    public Task LeaveAsync(int userId, string connectionId)
    {
        if (_conns.TryRemove(connectionId, out var tuple))
        {
            foreach (var kv in tuple.Item2.Keys)
            {
                if (_convoUsers.TryGetValue(kv, out var users))
                {
                    users.TryRemove(userId, out _);
                    if (users.IsEmpty) _convoUsers.TryRemove(kv, out _);
                }
            }
        }
        return Task.CompletedTask;
    }

    public bool IsUserInConversation(int userId, Guid convoId)
        => _convoUsers.TryGetValue(convoId, out var users) && users.ContainsKey(userId);
}
