// wwwroot/WowDash/js/pwa-push.js
(() => {
    if (!('serviceWorker' in navigator) || !('PushManager' in window) || typeof Notification === 'undefined') return;

    const api = (p) => (window.__appBasePath || '') + (p.startsWith('/') ? p : '/' + p);

    // Expose controls for Prefs page buttons
    window.SwimsPush = {
        enable: async () => subscribeFlow({ prompt: true }),
        disable: async () => disableFlow(),
        status: async () => ({
            permission: Notification.permission,
            subscribed: !!(await getSubSafe())
        })
    };

    // ✅ KEEP your desired behavior: auto prompt + auto subscribe on page load.
    // Browser only prompts while permission is "default".
    subscribeFlow({ prompt: true });

    async function subscribeFlow({ prompt }) {
        try {
            // Wait for the SW that your layout registers on window.load
            const reg = await navigator.serviceWorker.ready;

            if (Notification.permission === 'default' && prompt) {
                const perm = await Notification.requestPermission();
                if (perm !== 'granted') return;
            }
            if (Notification.permission !== 'granted') return;

            // Fetch VAPID public key
            const r = await fetch(api('/api/v1/me/push/vapid'), { credentials: 'same-origin' });
            if (!r.ok) return;

            const { publicKey } = await r.json();
            if (!publicKey) return;

            const key = urlBase64ToUint8Array(publicKey);

            // ✅ Idempotent subscribe: reuse if already subscribed
            let sub = await reg.pushManager.getSubscription();
            if (!sub) {
                sub = await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: key });
            }

            const json = sub.toJSON();

            // Persist/refresh server-side subscription (your endpoint is already an upsert pattern)
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
    }

    async function disableFlow() {
        try {
            const reg = await navigator.serviceWorker.ready;
            const sub = await reg.pushManager.getSubscription();
            if (!sub) return;

            const endpoint = sub.endpoint;

            // Best-effort browser unsubscribe
            try { await sub.unsubscribe(); } catch { }

            // Best-effort server deactivate
            await fetch(api('/api/v1/me/push/unsubscribe'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify({ endpoint })
            });
        } catch (err) {
            console.warn('push disable failed', err);
        }
    }

    async function getSubSafe() {
        try {
            const reg = await navigator.serviceWorker.ready;
            return await reg.pushManager.getSubscription();
        } catch { return null; }
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