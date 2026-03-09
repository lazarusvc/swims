# Web Push (PWA)

SWIMS supports **Web Push notifications** via the VAPID protocol, enabling notifications to be delivered to users' devices even when the browser is not open. Combined with a Service Worker, SWIMS can be installed as a **Progressive Web App (PWA)**.

## VAPID Keys

VAPID (Voluntary Application Server Identification) requires a public/private key pair. Generate once:

```bash
# Using npx web-push CLI
npx web-push generate-vapid-keys
```

Set in configuration:

```json
"WebPush": {
  "Subject": "mailto:admin@yourdomain.com",
  "PublicKey": "<base64url VAPID public key>",
  "PrivateKey": "<base64url VAPID private key>"
}
```

> [!WARNING]
> VAPID keys are long-lived. If you regenerate them, all existing push subscriptions become invalid and users must re-subscribe.

## Subscription Flow

1. When a user enables web push in their notification preferences, the browser calls the Push API using the **public VAPID key**.
2. The browser returns a `PushSubscription` object containing `endpoint`, `p256dh`, and `auth` values.
3. The client POSTs this to `POST /me/push/subscribe`.
4. SWIMS persists the subscription in `notify.push_subscriptions`.

## `notify.push_subscriptions` Schema

```sql
CREATE TABLE notify.push_subscriptions (
    Id          BIGINT IDENTITY PRIMARY KEY,
    UserId      INT NOT NULL,
    Endpoint    NVARCHAR(512) NOT NULL UNIQUE,
    P256dh      NVARCHAR(256),
    Auth        NVARCHAR(64),
    UserAgent   NVARCHAR(256),
    IsActive    BIT NOT NULL DEFAULT 1,
    CreatedUtc  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LastSeenUtc DATETIME2
);
```

## Sending Push Notifications

`WebPushSender.SendToUserAsync(userId, payload)`:

1. Loads all active subscriptions for the user from DB.
2. Sends the payload to each subscription endpoint using `Lib.Net.Http.WebPush`.
3. On `404` or `410` HTTP error (subscription expired/deleted): marks the subscription `IsActive = false` — no retry.
4. All push errors are swallowed — push is best-effort and must not break the notification flow.

### Push Payload Format

```json
{
  "title": "SWIMS",
  "body": "You have a new message from Alice Peters",
  "url": "https://swims.example.com/Portal/Messenger/Chat?convoId=...",
  "tag": "NewMessage"
}
```

`tag` is used by the browser to deduplicate/replace notifications of the same type.

## Service Worker

A `service-worker.js` file in `wwwroot/` handles push events and displays native notifications. On notification click, it opens the SWIMS URL from the payload.

## PWA Manifest

`wwwroot/site.webmanifest` declares the PWA identity: app name, icons, theme colour, display mode (`standalone`). When accessed from a compatible browser, users are prompted to "Add to Home Screen."

## REST Endpoints

| Route | Purpose |
|-------|---------|
| `POST /me/push/subscribe` | Register a new push subscription |
| `DELETE /me/push/subscribe` | Unregister a subscription (by endpoint) |
| `GET /me/push/vapid-public-key` | Expose VAPID public key to client |
