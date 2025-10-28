/* SWIMS Service Worker (collapse/replace notifications) */

const CACHE = 'swims-core-v1';

/** Helper: absolute URL relative to SW scope */
const abs = (u) => new URL(u, self.registration.scope).href;

/** Compute app BASE from the SW scope (e.g., "/swims-test") */
const BASE = new URL(self.registration.scope).pathname.replace(/\/$/, '');

/* --------------------------------------------------------------------------
 * INSTALL: take control immediately & pre-cache minimal app shell
 * -------------------------------------------------------------------------- */
self.addEventListener('install', (event) => {
    self.skipWaiting();
    const CORE = [
        `${BASE}/`,
        `${BASE}/manifest.webmanifest`
    ];
    event.waitUntil((async () => {
        try {
            const cache = await caches.open(CACHE);
            await cache.addAll(CORE);
        } catch (_err) {
            // Ignore (e.g., network unavailable or manifest missing in certain envs)
        }
    })());
});

/* --------------------------------------------------------------------------
 * ACTIVATE: claim clients & clean old caches
 * -------------------------------------------------------------------------- */
self.addEventListener('activate', (event) => {
    self.clients.claim();
    const keep = [CACHE];
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(keys.map(k => keep.includes(k) ? null : caches.delete(k)));
    })());
});

/* --------------------------------------------------------------------------
 * FETCH: network-first with cache fallback for static assets only
 * Avoid caching authenticated API/HTML content.
 * -------------------------------------------------------------------------- */
self.addEventListener('fetch', (event) => {
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);
    const sameOrigin = url.origin === self.location.origin;
    const dest = event.request.destination; // 'script' | 'style' | 'image' | 'font' | 'document' | ...

    // Only cache static same-origin assets
    const cacheable = sameOrigin && ['script', 'style', 'image', 'font'].includes(dest);

    event.respondWith((async () => {
        try {
            const res = await fetch(event.request);
            if (cacheable && res.ok) {
                const cache = await caches.open(CACHE);
                cache.put(event.request, res.clone());
            }
            return res;
        } catch {
            const cache = await caches.open(CACHE);
            const cached = await cache.match(event.request);
            if (cached) return cached;

            // For navigations, fallback to the app root if available
            if (dest === 'document' && sameOrigin) {
                const shell = await cache.match(`${BASE}/`);
                if (shell) return shell;
            }

            // Let the error propagate (you can customize an offline response here)
            throw new Error('Network error and no cached response.');
        }
    })());
});

/* --------------------------------------------------------------------------
 * PUSH: show a notification that collapses/replaces by tag
 * - Default: tag 'swims' (all pushes replace each other)
 * - Server can override with { tag: 'something' } to group by category
 * - Re-alert (renotify) defaults to true; override with { renotify: false }
 * -------------------------------------------------------------------------- */
self.addEventListener('push', (event) => {
    let data = {};
    try { data = event.data ? event.data.json() : {}; } catch { /* ignore */ }

    const title = data.title || 'SWIMS';

    // Fixed tag collapses/updates prior notification(s).
    // Allow server to set a category tag, e.g., 'alerts', 'chat', etc.
    const tag = (typeof data.tag === 'string' && data.tag) ? data.tag : 'swims';

    // Re-alert user when the same tag updates (default true, can be disabled)
    const renotify = (data.renotify !== undefined) ? !!data.renotify : true;

    const targetUrl = abs(data.url || `${BASE}/`);

    const options = {
        body: data.body || '',
        tag,
        renotify,
        data: {
            url: targetUrl,
            // pass through any extra metadata your app needs
            ...data.data,
        },
        icon: `${BASE}/icons/sw-icon-192.png`,
        badge: `${BASE}/icons/sw-icon-192.png`,
        requireInteraction: !!data.requireInteraction, // stays until user acts (optional)
        silent: !!data.silent,                         // no sound/vibration (optional)
        timestamp: Date.now(),
        actions: Array.isArray(data.actions) ? data.actions : undefined
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

/* --------------------------------------------------------------------------
 * NOTIFICATION CLICK: focus existing tab or open target URL
 * -------------------------------------------------------------------------- */
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const url = (event.notification.data && event.notification.data.url) ? event.notification.data.url : abs(`${BASE}/`);

    event.waitUntil((async () => {
        const windows = await clients.matchAll({ type: 'window', includeUncontrolled: true });

        // Prefer an existing tab for this app base
        const basePrefix = self.location.origin + BASE;
        const existing = windows.find(c => typeof c.url === 'string' && c.url.startsWith(basePrefix));

        if (existing) {
            await existing.focus();
            if (('navigate' in existing) && existing.url !== url) {
                try { await existing.navigate(url); } catch { }
            }
            return;
        }

        await clients.openWindow(url);
    })());
});

/* --------------------------------------------------------------------------
 * OPTIONAL: allow app to trigger skipWaiting on updated SW
 * In your page: navigator.serviceWorker.controller.postMessage({ type: 'SKIP_WAITING' })
 * -------------------------------------------------------------------------- */
self.addEventListener('message', (event) => {
    if (!event.data) return;
    if (event.data.type === 'SKIP_WAITING') self.skipWaiting();
});
