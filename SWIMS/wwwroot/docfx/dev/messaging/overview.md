# Messaging — Overview

SWIMS includes a **real-time 1:1 chat** system built on ASP.NET Core SignalR. Users can exchange messages with colleagues directly within the application.

## Architecture

```
Browser (chat.js)
  │
  ├── SignalR WebSocket ──► ChatsHub (/hubs/chats)
  │                             └── persists to msg.* tables
  │
  └── REST API ──► MessagingEndpoints (/api/v1/me/chats/*)
                       └── reads from msg.* tables
```

## Data Model

### `msg.conversations`

```sql
Id          UNIQUEIDENTIFIER PK
CreatedAt   DATETIME2
LastMessageAt DATETIME2
```

### `msg.conversation_members`

```sql
ConversationId  UNIQUEIDENTIFIER FK
UserId          INT
JoinedAt        DATETIME2
LastReadAt      DATETIME2       -- for unread count tracking
```

### `msg.messages`

```sql
Id              UNIQUEIDENTIFIER PK
ConversationId  UNIQUEIDENTIFIER FK
SenderId        INT              FK → users
Body            NVARCHAR(MAX)
CreatedAt       DATETIME2
IsDeleted       BIT DEFAULT 0
```

## ChatsHub

**Route**: `/hubs/chats`

| Hub Method (client → server) | Description |
|------------------------------|-------------|
| `SendMessage(convoId, body)` | Send a message to a conversation |
| `JoinConversation(convoId)` | Join a SignalR group for a conversation |
| `LeaveConversation(convoId)` | Leave a conversation group |
| `MarkRead(convoId)` | Update `LastReadAt` for the current user |

| Hub Event (server → client) | Description |
|-----------------------------|-------------|
| `ReceiveMessage` | New message payload: `{id, convoId, senderId, senderName, body, createdAt}` |
| `PresenceUpdate` | Online/offline status change for a conversation member |

## Chat Presence

`IChatPresence` / `InMemoryChatPresence` tracks which users are currently online in the hub. Presence state is **in-memory only** — it resets on app restart and is not shared across multiple server instances.

> [!NOTE]
> For multi-instance deployments (load balanced), replace `InMemoryChatPresence` with a Redis-backed implementation. SignalR's backplane also needs to be configured (Azure SignalR Service or Redis backplane).

## REST Endpoints

| Route | Purpose |
|-------|---------|
| `GET /me/chats` | List conversations for the current user |
| `GET /me/chats/{convoId}/messages?skip=&take=` | Paginated message history |
| `POST /me/chats` | Create a new conversation (with initial member list) |
| `POST /me/chats/{convoId}/members` | Add a member to a conversation |

## Notification Integration

When a message is sent via `ChatsHub.SendMessage`, the hub calls `INotifier.NotifyUserAsync` for each conversation member (except the sender), triggering:

- An in-app notification (bell)
- A web push notification (if subscribed)
- An email notification (if user preferences allow)

Notification payload for `NewMessage`:

```json
{
  "messageId": "guid",
  "fromUserId": 123,
  "fromName": "Alice Peters",
  "convoId": "guid",
  "snippet": "first 120 chars of message body...",
  "url": "https://host/Portal/Messenger/Chat?convoId=...",
  "actionLabel": "Open chat"
}
```

## Portal Pages

| Page | Route |
|------|-------|
| Conversation list | `/Portal/Messenger/Chat` |
| Chat window | `/Portal/Messenger/Chat?convoId={id}` |
| User profile | `/Portal/Messenger/ChatProfile?userId={id}` |
