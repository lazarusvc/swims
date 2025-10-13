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
    try { data = e.data ? e.data.json() : {}; } catch { data = { title: 'Notification', body: e.data?.text() }; }
    const title = data.title || 'SWIMS';
    const body = data.body || '';
    const url = data.url || '/';
    const tag = data.tag || 'swims';
    e.waitUntil(self.registration.showNotification(title, { body, tag, data: { url } }));
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
