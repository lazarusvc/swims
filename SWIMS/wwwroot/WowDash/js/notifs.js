// wwwroot/WowDash/js/notifs.js
(function () {
    const onReady = (fn) => {
        if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', fn);
        else fn();
    };

    onReady(() => {
        // Elements
        const bell = document.getElementById('notif-bell');
        const menu = document.getElementById('notif-dropdown');
        const list = document.getElementById('notif-list');
        const badge = document.getElementById('notif-badge');
        const badgeH = document.getElementById('notif-badge-header'); // header bubble (optional)
        const btnAll = document.getElementById('notif-mark-all');
        const btnMore = document.getElementById('notif-load-more');

        if (!bell || !menu || !list || !badge) return; // not on this page/layout

        // State
        let skip = 0, take = 10, total = 0, items = [], loading = false;

        // Utils
        const parseJSON = (s, fallback = {}) => { try { return s ? JSON.parse(s) : fallback; } catch { return fallback; } };
        const fmtTime = (iso) => {
            try { return new Date(iso).toLocaleString(); } catch { return iso || ''; }
        };
        const setBadge = (count) => {
            if (count > 0) {
                badge.textContent = String(count);
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
            if (badgeH) badgeH.textContent = String(count || 0);
        };

        const liHtml = (n) => {
            const payload = parseJSON(n.payloadJson);
            const msg = payload.message || n.type;
            const seenClass = n.seen ? 'opacity-75' : 'bg-neutral-50';
            return `
        <li data-id="${n.id}">
          <a href="javascript:void(0)"
             class="px-24 py-12 d-flex align-items-start gap-3 mb-2 justify-content-between ${seenClass}">
            <div class="text-black hover-bg-transparent hover-text-primary d-flex align-items-center gap-3">
              <span class="w-44-px h-44-px bg-info-subtle text-info-main rounded-circle d-flex justify-content-center align-items-center flex-shrink-0">
                <iconify-icon icon="bitcoin-icons:verify-outline" class="icon text-xxl"></iconify-icon>
              </span>
              <div>
                <h6 class="text-md fw-semibold mb-4 mb-1">${msg}</h6>
                <p class="mb-0 text-sm text-secondary-light text-w-200-px">${fmtTime(n.createdUtc)}</p>
              </div>
            </div>
          </a>
        </li>`;
        };

        const render = () => {
            list.innerHTML = items.map(liHtml).join('');
            btnMore && (btnMore.style.display = (skip < total) ? '' : 'none');
        };

        const load = async (more = false) => {
            if (loading) return;
            loading = true;
            try {
                if (!more) { skip = 0; items = []; total = 0; }
                const r = await fetch(`/me/notifications?unseenOnly=false&skip=${skip}&take=${take}`, { credentials: 'same-origin' });
                if (!r.ok) throw new Error('Failed to load notifications');
                const data = await r.json();
                total = data.total || 0;
                const newItems = Array.isArray(data.items) ? data.items : [];
                skip += newItems.length;
                items = items.concat(newItems);
                render();
            } catch (e) {
                // eslint-disable-next-line no-console
                console.warn('[notifs] load failed:', e);
            } finally {
                loading = false;
            }
        };

        const refreshCount = async () => {
            try {
                const r = await fetch('/me/notifications/count', { credentials: 'same-origin' });
                if (!r.ok) throw 0;
                const data = await r.json();
                setBadge(Number(data.count || 0));
            } catch {
                // fallback: compute from current items (approximate)
                const c = items.reduce((acc, n) => acc + (n.seen ? 0 : 1), 0);
                setBadge(c);
            }
        };

        // --- Bootstrap dropdown events ---
        // Load when dropdown is shown
        bell.addEventListener('shown.bs.dropdown', async () => {
            await load(false);
            await refreshCount();
        });

        // Optional: refresh count when dropdown hides
        bell.addEventListener('hidden.bs.dropdown', async () => {
            await refreshCount();
        });

        // Mark all as read
        btnAll && btnAll.addEventListener('click', async () => {
            try {
                await fetch('/me/notifications/seen-all', { method: 'POST', credentials: 'same-origin' });
                items = items.map(n => ({ ...n, seen: true }));
                render();
                setBadge(0);
            } catch (e) {
                console.warn('[notifs] mark-all failed:', e);
            }
        });

        // Load more
        btnMore && btnMore.addEventListener('click', () => load(true));

        // Mark one on click (event delegation)
        list.addEventListener('click', async (e) => {
            const li = e.target.closest('li[data-id]');
            if (!li) return;
            const id = li.getAttribute('data-id');
            try {
                await fetch(`/me/notifications/${id}/seen`, { method: 'POST', credentials: 'same-origin' });
                const idx = items.findIndex(n => String(n.id) === String(id));
                if (idx >= 0) items[idx].seen = true;
                li.querySelector('a')?.classList.remove('bg-neutral-50');
                li.querySelector('a')?.classList.add('opacity-75');
                await refreshCount();
            } catch (e2) {
                console.warn('[notifs] mark-one failed:', e2);
            }
        });

        // --- SignalR live updates ---
        if (window.signalR) {
            const conn = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/notifs', { withCredentials: true })
                .withAutomaticReconnect()
                .build();

            conn.on('notif', async (n) => {
                // Normalize and prepend
                items.unshift({
                    id: n.id, type: n.type, payloadJson: n.payloadJson,
                    createdUtc: n.createdUtc, seen: false
                });
                render();
                // bump badge
                await refreshCount();
            });

            conn.start().catch(err => console.error('[notifs] signalR failed:', err));
        }

        // initial badge
        refreshCount().catch(() => { });
    });
})();
