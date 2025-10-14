self.addEventListener('install', (e) => { self.skipWaiting(); });
self.addEventListener('activate', (e) => { self.clients.claim(); });

// Basic cache (optional to expand)
const CORE = ['/', '/manifest.webmanifest'];
self.addEventListener('fetch', (e) => {
    if (e.request.method !== 'GET') return;
    e.respondWith(fetch(e.request).catch(() => caches.open('swims-core').then(c => c.match(e.request))));
});

// Web Push
self.addEventListener('push', (e) => {
    let data = {};
    try { data = e.data ? e.data.json() : {}; } catch { data = {}; }

    const title = data.title || 'SWIMS';
    const body = data.body || '';
    const url = data.url || '/';

    // If no tag provided, generate a unique one so every push shows
    const tag = data.tag || `swims-${Date.now()}-${Math.random().toString(36).slice(2)}`;

    const options = {
        body,
        tag,
        renotify: data.renotify !== false, // default true unless explicitly disabled
        requireInteraction: false,         // set true if you want it to stick until clicked
        data: { url },
        badge: '/icons/icon-192.png',
        icon: '/icons/icon-192.png',
        timestamp: Date.now()
    };

    e.waitUntil(self.registration.showNotification(title, options));
});


self.addEventListener('notificationclick', (e) => {
    e.notification.close();
    const url = e.notification.data?.url || '/';
    e.waitUntil((async () => {
        const all = await clients.matchAll({ type: 'window', includeUncontrolled: true });
        const tab = all.find(w => w.url.includes(url));
        if (tab) { tab.focus(); } else { clients.openWindow(url); }
    })());
});
