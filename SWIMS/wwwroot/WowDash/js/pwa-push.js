(async () => {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return;

    const api = (p) => (window.__appBasePath || '') + (p.startsWith('/') ? p : '/' + p);


    try {
        const reg = await navigator.serviceWorker.getRegistration();
        if (!reg) return; // service worker not registered yet

        const perm = await Notification.requestPermission();
        if (perm !== 'granted') return;

        const r = await fetch(api('/api/v1/me/push/vapid'), { credentials: 'same-origin' });
        if (!r.ok) return;
        const { publicKey } = await r.json();
        if (!publicKey) return;

        const key = urlBase64ToUint8Array(publicKey);
        const sub = await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: key });
        const json = sub.toJSON(); // { endpoint, keys:{ p256dh, auth } }

        await fetch(api('/api/v1/me/push/subscribe'), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify({
                endpoint: json.endpoint,
                p256dh: json.keys?.p256dh,
                auth: json.keys?.auth
            })
        });
    } catch (err) {
        console.warn('push subscribe failed', err);
    }

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const rawData = atob(base64);
        const out = new Uint8Array(rawData.length);
        for (let i = 0; i < rawData.length; ++i) out[i] = rawData.charCodeAt(i);
        return out;
    }
})();
