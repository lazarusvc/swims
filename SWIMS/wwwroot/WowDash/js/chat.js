(() => {
    if (window.__swims_chats_started) return;

    // Only run on pages that actually have the chat UI
    const chatRoot = document.getElementById('chat-convos');
    if (!chatRoot) return;
    window.__swims_chats_started = true;

    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));
    const api = (p) => (window.__appBasePath || '') + (p.startsWith('/') ? p : '/' + p);

    const qsAny = (selectors) => {
        for (const s of selectors.split(',')) {
            const el = document.querySelector(s.trim());
            if (el) return el;
        }
        return null;
    };

    const state = { me: null, convoId: null, messages: [], hub: null, peers: {} };

    async function fetchJson(url, opts = {}) {
        const r = await fetch(url, {
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json', ...(opts.headers || {}) },
            ...opts
        });
        if (!r.ok) {
            let msg; try { msg = await r.json(); } catch { msg = await r.text(); }
            const text = (msg && msg.error) ? msg.error : (typeof msg === 'string' ? msg : `HTTP ${r.status}`);
            throw new Error(text);
        }
        const ct = (r.headers.get('content-type') || '').toLowerCase();
        if (!ct.includes('application/json')) throw new Error('Unexpected content type from server.');
        return r.json();
    }

    const esc = (s) => (s || '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const debounce = (fn, ms = 200) => { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; };

    // ---------- render ----------
    function setHeader(convoId) {
        const peer = state.peers[convoId] || null;
        const nameEl = $('#chat-peer-name');
        const subEl = $('#chat-peer-sub');
        if (nameEl) nameEl.textContent = peer ? (peer.displayName || '(unknown)') : 'Select a conversation';
        if (subEl) subEl.textContent = (peer && peer.email) ? `${peer.email}` : '';
    }

    function renderConvos(d) {
        const ul = $('#chat-convos'); if (!ul) return;

        // store peers for header use
        state.peers = {};
        (d.items || []).forEach(x => {
            const p = x.other || {};
            state.peers[x.id] = {
                userId: p.userId,
                username: p.username,
                email: p.email,
                firstName: p.firstName,
                lastName: p.lastName,
                displayName: p.displayName || p.username || p.email || '(unknown)'
            };
        });

        ul.innerHTML = (d.items || []).map(x => {
            const p = state.peers[x.id];
            const name = p?.displayName || '(unknown)';
            const sub = x.lastMessage ? (x.lastMessage.body || '').slice(0, 50) : '';
            return `<li data-id="${x.id}" class="chat-convo ${x.unread ? 'has-unread' : ''}">
        <div class="name">${esc(name)}</div>
        <div class="sub">${esc(sub)}</div>
        ${x.unread ? `<span class="badge bg-primary">${x.unread}</span>` : ''}
      </li>`;
        }).join('');

        $$('#chat-convos .chat-convo').forEach(li => li.onclick = () => openConvo(li.dataset.id));
    }

    function renderMessages(items, { append = false } = {}) {
        const pane = $('#chat-thread'); if (!pane) return;
        if (!append) pane.innerHTML = '';
        for (const m of items) {
            const mine = (m.senderUserId ?? m.fromUserId) === state.me;
            const div = document.createElement('div');
            div.className = 'chat-msg ' + (mine ? 'me' : 'them');
            div.innerHTML = `<div class="bubble"><div class="body">${esc(m.body)}</div>
                       <div class="meta">${new Date(m.createdUtc).toLocaleString()}</div></div>`;
            pane.appendChild(div);
        }
        pane.scrollTop = pane.scrollHeight;
    }

    // ---------- data ----------
    async function loadConvos() {
        const d = await fetchJson(api('/api/v1/me/chats?skip=0&take=50'));
        renderConvos(d);
        return d;
    }

    async function openConvo(id) {
        state.convoId = id;
        setHeader(id);

        await ensureHub();
        try { await state.hub.invoke('Join', id); } catch { /* no-op */ }
        const d = await fetchJson(api(`/api/v1/me/chats/${id}/messages?take=50`));
        state.messages = d.items || [];
        renderMessages(state.messages);
        await updateRead();
    }

    async function startWithLogin(login) {
        const data = await fetchJson(api(`/api/v1/me/chats/start/login?login=${encodeURIComponent(login)}`), { method: 'POST' });
        await openConvo(data.id);
        await loadConvos();
        setHeader(data.id);
    }

    async function sendMessage() {
        const box = $('#chat-input'); if (!box || !state.convoId) return;
        const body = (box.value || '').trim();
        if (!body) return;
        await fetchJson(api(`/api/v1/me/chats/${state.convoId}/messages`), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ body })
        });
        box.value = '';
    }

    async function updateRead() {
        if (!state.convoId || !state.messages.length) return;
        const last = state.messages[state.messages.length - 1];
        await fetchJson(api(`/api/v1/me/chats/${state.convoId}/read`), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ lastReadMessageId: last.id })
        });
    }

    // ---------- hub ----------
    async function ensureHub() {
        if (state.hub) return;
        if (!window.signalR || !window.signalR.HubConnectionBuilder) {
            console.error('SignalR not loaded.');
            return;
        }
        state.hub = new signalR.HubConnectionBuilder()
            .withUrl(api('/hubs/chats'), { withCredentials: true })
            .withAutomaticReconnect()
            .build();

        state.hub.on('message', (m) => {
            if (m.convoId !== state.convoId) { loadConvos(); return; }
            const item = { id: m.id, conversationId: m.convoId, senderUserId: m.fromUserId, body: m.body, createdUtc: m.createdUtc };
            state.messages.push(item);
            renderMessages([item], { append: true });
            updateRead();
        });

        state.hub.on('read', (_e) => { /* read receipts later */ });

        await state.hub.start();
    }

    // ---------- typeahead ----------
    function attachTypeahead() {
        const input = qsAny('#chat-start-login,#chat-start-user,#chat-start-email,[data-chat-start-input]');
        if (!input) return;

        const menu = document.createElement('div');
        menu.id = 'chat-people-menu';
        menu.className = 'dropdown-menu show';
        menu.style.position = 'absolute';
        menu.style.minWidth = '240px';
        menu.style.maxHeight = '240px';
        menu.style.overflowY = 'auto';
        menu.style.background = '#fff';
        menu.style.border = '1px solid rgba(0,0,0,.15)';
        menu.style.borderRadius = '0.5rem';
        menu.style.boxShadow = '0 .5rem 1rem rgba(0,0,0,.15)';
        menu.style.zIndex = 1055;
        menu.style.display = 'none';
        document.body.appendChild(menu);

        const posMenu = () => {
            const r = input.getBoundingClientRect();
            menu.style.left = (window.scrollX + r.left) + 'px';
            menu.style.top = (window.scrollY + r.bottom + 2) + 'px';
            menu.style.width = r.width + 'px';
        };

        const show = (html) => { menu.innerHTML = html; posMenu(); menu.style.display = 'block'; };
        const hide = () => { menu.style.display = 'none'; };

        const search = debounce(async (q) => {
            q = (q || '').trim();
            if (!q) { hide(); return; }
            let d;
            try {
                d = await fetchJson(api(`/api/v1/me/chats/users/search?q=${encodeURIComponent(q)}&take=8`));
            } catch { hide(); return; }
            const items = d.items || [];
            if (!items.length) { hide(); return; }
            const html = items.map(u => `
        <button type="button" class="dropdown-item w-100 text-start" data-login="${esc(u.email || u.username)}">
          <div class="fw-semibold">${esc(u.firstName && u.lastName ? (u.firstName + ' ' + u.lastName) : (u.username || '(no username)'))}</div>
          <div class="small text-muted">${esc(u.email || '')}</div>
        </button>`).join('');
            show(html);
            $$('button.dropdown-item', menu).forEach(b => {
                b.onclick = async () => {
                    input.value = b.dataset.login || '';
                    hide();
                    const btnStart = qsAny('#chat-start,[data-chat-start]');
                    if (btnStart) btnStart.click();
                };
            });
        }, 200);

        input.addEventListener('input', () => search(input.value));
        input.addEventListener('focus', () => { if (input.value) search(input.value); });
        window.addEventListener('resize', posMenu);
        window.addEventListener('scroll', posMenu, true);
        document.addEventListener('click', (e) => {
            if (e.target === input || menu.contains(e.target)) return;
            hide();
        });
    }

    // ---------- boot ----------
    function initMe() {
        const me = document.querySelector('meta[name="swims-user-id"]')?.content;
        state.me = me ? parseInt(me, 10) : null;
    }

    function wireUi() {
        const btnSend = qsAny('#chat-send,[data-chat-send]');
        const inputNew = qsAny('#chat-start-login,#chat-start-user,#chat-start-email,[data-chat-start-input]');
        const btnStart = qsAny('#chat-start,[data-chat-start]');
        const box = qsAny('#chat-input,[data-chat-input]');

        if (btnSend && box) {
            btnSend.onclick = sendMessage;
            box.onkeydown = (e) => {
                if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendMessage(); }
            };
        }

        if (btnStart && inputNew) {
            btnStart.onclick = async () => {
                const v = (inputNew.value || '').trim();
                if (!v) return;
                try { await startWithLogin(v); }
                catch (err) { alert(`Could not start chat: ${err.message || err}`); }
            };
            inputNew.onkeydown = (e) => { if (e.key === 'Enter') { e.preventDefault(); btnStart.click(); } };
        }
    }

    async function main() {
        initMe();
        wireUi();
        attachTypeahead();

        const list = await loadConvos();

        if (window.__chatOpenConvoId) {
            await openConvo(window.__chatOpenConvoId);
        } else if (window.__chatStartUserId) {
            const data = await fetchJson(api(`/api/v1/me/chats/start?userId=${encodeURIComponent(window.__chatStartUserId)}`), { method: 'POST' });
            await openConvo(data.id);
            await loadConvos();
            setHeader(data.id);
        } else if ((list.items || []).length) {
            await openConvo(list.items[0].id);
        }
    }

    main().catch(err => console.error(err));
})();
