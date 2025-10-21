(function () {
    const onReady = (fn) => (document.readyState === 'loading')
        ? document.addEventListener('DOMContentLoaded', fn) : fn();

    const api = (p) => (window.__appBasePath || '') + (p.startsWith('/') ? p : '/' + p);


    onReady(async () => {
        const rowsEl = document.getElementById('pref-rows');
        const gIn = document.getElementById('pref-global-inapp');
        const gEm = document.getElementById('pref-global-email');
        const gDi = document.getElementById('pref-global-digest');
        if (!rowsEl || !gIn || !gEm || !gDi) return;

        // Fetch helpers
        const jget = async (u) => (await fetch(u, { credentials: 'same-origin' })).json();
        const jput = async (u, body) => {
            const r = await fetch(u, {
                method: 'PUT',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!r.ok) throw new Error('Save failed');
            return r.json();
        };

        // Load data
        const [types, prefs] = await Promise.all([
            jget(api('/api/v1/me/notifications/types')),
            jget(api('/api/v1/me/notifications/prefs'))

        ]);

        // Build a map of prefs (type->flags) plus global
        const prefMap = new Map();
        let global = { inApp: true, email: false, digest: false };
        for (const p of prefs) {
            if (p.type == null) global = { inApp: p.inApp, email: p.email, digest: p.digest };
            else prefMap.set(p.type, { inApp: p.inApp, email: p.email, digest: p.digest });
        }

        // Apply global to UI
        gIn.checked = !!global.inApp;
        gEm.checked = !!global.email;
        gDi.checked = !!global.digest;

        const saveGlobal = async () => {
            await jput(api('/api/v1/me/notifications/prefs'), { type: null, inApp: gIn.checked, email: gEm.checked, digest: gDi.checked });
        };
        gIn.addEventListener('change', saveGlobal);
        gEm.addEventListener('change', saveGlobal);
        gDi.addEventListener('change', saveGlobal);

        // Render per-type rows
        const mkRow = (t) => {
            const cur = prefMap.get(t) || global; // default to global if no override
            const id = (s) => `pref-${t}-${s}`.replace(/[^a-z0-9_-]/gi, '_');

            return `
        <tr data-type="${t}">
          <td><code>${t}</code></td>
          <td><input id="${id('in')}" class="form-check-input" type="checkbox" ${cur.inApp ? 'checked' : ''}></td>
          <td><input id="${id('em')}" class="form-check-input" type="checkbox" ${cur.email ? 'checked' : ''}></td>
          <td><input id="${id('di')}" class="form-check-input" type="checkbox" ${cur.digest ? 'checked' : ''}></td>
        </tr>`;
        };

        rowsEl.innerHTML = types.map(mkRow).join('');

        // Attach change handlers (event delegation)
        rowsEl.addEventListener('change', async (e) => {
            const tr = e.target.closest('tr[data-type]');
            if (!tr) return;
            const type = tr.getAttribute('data-type');
            const inputs = tr.querySelectorAll('input.form-check-input');
            const [inapp, email, digest] = [...inputs].map(i => i.checked);
            try {
                await jput(api('/api/v1/me/notifications/prefs'), { type, inApp: inapp, email, digest });
            } catch (err) {
                console.warn('save failed', err);
            }
        });
    });
})();
