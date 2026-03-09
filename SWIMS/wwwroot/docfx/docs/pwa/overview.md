# PWA & Web Push

SWIMS can be installed as a **Progressive Web App (PWA)** on desktop and mobile devices, and supports native browser push notifications via the Web Push protocol.

See [Notifications → Web Push](../notifications/web-push.md) for full technical details on the VAPID keys, subscription flow, and push delivery.

## PWA Features

| Feature | Status |
|---------|--------|
| Web App Manifest (`site.webmanifest`) | ✅ |
| Service Worker (offline support) | ✅ (basic) |
| Installable (Add to Home Screen) | ✅ |
| Push Notifications | ✅ |
| Background Sync | ❌ Not yet implemented |

## Web App Manifest

`wwwroot/site.webmanifest`:

```json
{
  "name": "SWIMS",
  "short_name": "SWIMS",
  "description": "Social Welfare Information Management System",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#1B4F8A",
  "theme_color": "#1B4F8A",
  "icons": [
    { "src": "/icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

## Service Worker

`wwwroot/service-worker.js` handles:

- **Push event**: displays a native notification using the payload title/body/url from the push message.
- **Notification click**: focuses the SWIMS tab or opens the action URL.
- **Fetch events**: basic caching strategy for static assets (network-first for API, cache-first for assets).

## Installing SWIMS

In compatible browsers (Chrome, Edge, Safari 17+):

1. Open SWIMS in the browser.
2. Look for the **"Install"** icon in the address bar (desktop) or **"Add to Home Screen"** in the browser menu (mobile).
3. Confirm installation.
4. SWIMS opens in a standalone window without browser chrome.

Once installed, users can enable **Push Notifications** from their Notification Preferences page (`/Portal/Notifications/Prefs`) to receive alerts even when SWIMS is not open.

## VAPID Key Requirements

Push notifications require VAPID keys to be configured in `appsettings.json`. See [Web Push](../notifications/web-push.md) for key generation and configuration instructions.
