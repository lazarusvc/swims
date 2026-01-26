(function () {
    const HOST_ID = "swims-toast-stack";

    function safeJsonParse(s) {
        try { return JSON.parse(s); } catch { return null; }
    }

    function humanizeType(type) {
        if (!type) return "Notification";
        return String(type)
            .replace(/[_\-]+/g, " ")
            .replace(/([a-z])([A-Z])/g, "$1 $2")
            .trim();
    }

    function formatRelativeTime(iso) {
        if (!iso) return "Just now";
        const dt = new Date(iso);
        if (isNaN(dt.getTime())) return "Just now";

        const sec = Math.floor((Date.now() - dt.getTime()) / 1000);
        if (sec < 10) return "Just now";
        if (sec < 60) return `${sec}s ago`;
        const min = Math.floor(sec / 60);
        if (min < 60) return `${min}m ago`;
        const hr = Math.floor(min / 60);
        if (hr < 24) return `${hr}h ago`;
        const day = Math.floor(hr / 24);
        return `${day}d ago`;
    }

    function computeToastTopOffset() {
        const nav = document.querySelector(".navbar-header");
        // Fallback if markup changes
        const el = nav || document.querySelector("nav.navbar") || document.querySelector(".dashboard-main .navbar-header");
        if (!el) return;

        const rect = el.getBoundingClientRect();
        // rect.bottom is the “bottom edge” in viewport coords
        const top = Math.max(rect.bottom, 0) + 12;
        document.documentElement.style.setProperty("--swims-toast-top", `${top}px`);
    }

    function initToastOffset() {
        computeToastTopOffset();

        window.addEventListener("resize", computeToastTopOffset, { passive: true });
        window.addEventListener("scroll", computeToastTopOffset, { passive: true });

        const nav = document.querySelector(".navbar-header");
        if (nav && window.ResizeObserver) {
            const ro = new ResizeObserver(() => computeToastTopOffset());
            ro.observe(nav);
        }
    }

    function kindMeta(kind) {
        switch ((kind || "").toLowerCase()) {
            case "success":
                return { dot: "bg-success-600", icon: "bi:patch-check", text: "text-success-600" };
            case "warning":
                return { dot: "bg-warning-600", icon: "mdi:alert-outline", text: "text-warning-600" };
            case "danger":
            case "error":
                return { dot: "bg-danger-600", icon: "mingcute:delete-2-line", text: "text-danger-600" };
            default:
                return { dot: "bg-info-600", icon: "iconoir:bell", text: "text-info-600" };
        }
    }

    function openNotifDropdown() {
        const bell = document.getElementById("notif-bell");
        if (!bell) return;

        if (window.bootstrap?.Dropdown) {
            window.bootstrap.Dropdown.getOrCreateInstance(bell).show();
        } else {
            // Fallback: click
            bell.click();
        }
        bell.focus();
    }

    function buildToastEl({ title, message, kind, timeText, url, actionLabel, timeoutMs, onOpen }) {
        const host = document.getElementById(HOST_ID);
        if (!host) return null;

        const meta = kindMeta(kind);

        const toast = document.createElement("div");
        toast.className = "toast border-0 shadow-lg radius-12 bg-base mb-12";
        toast.setAttribute("role", "alert");
        toast.setAttribute("aria-live", "assertive");
        toast.setAttribute("aria-atomic", "true");
        toast.dataset.bsAutohide = "true";
        toast.dataset.bsDelay = String(timeoutMs ?? 6500);

        const header = document.createElement("div");
        header.className = "toast-header border-0 bg-base py-12 px-16 d-flex align-items-center gap-8";

        const dot = document.createElement("span");
        dot.className = `w-8-px h-8-px rounded-circle ${meta.dot}`;

        const icon = document.createElement("iconify-icon");
        icon.setAttribute("icon", meta.icon);
        icon.className = `icon text-lg ${meta.text}`;

        const strong = document.createElement("strong");
        strong.className = "me-auto text-secondary-light fw-semibold text-sm";
        strong.textContent = title || "Notification";

        const time = document.createElement("small");
        time.className = "text-secondary-light text-xs";
        time.textContent = timeText || "Just now";

        const closeBtn = document.createElement("button");
        closeBtn.type = "button";
        closeBtn.className = `remove-button ${meta.text} text-xxl line-height-1 ms-8`;
        closeBtn.setAttribute("aria-label", "Close");

        const closeIcon = document.createElement("iconify-icon");
        closeIcon.setAttribute("icon", "iconamoon:sign-times-light");
        closeIcon.className = "icon";
        closeBtn.appendChild(closeIcon);

        header.appendChild(dot);
        header.appendChild(icon);
        header.appendChild(strong);
        header.appendChild(time);
        header.appendChild(closeBtn);

        const body = document.createElement("div");
        body.className = "toast-body pt-0 pb-12 px-16";

        const msg = document.createElement("div");
        msg.className = "text-secondary-light text-sm";
        msg.textContent = message || "";
        body.appendChild(msg);

        // Action row
        const actionRow = document.createElement("div");
        actionRow.className = "mt-8";

        if (url) {
            const a = document.createElement("a");
            a.className = "text-primary-600 fw-semibold text-sm";
            a.href = url;
            a.textContent = actionLabel || "View";
            actionRow.appendChild(a);
        } else if (typeof onOpen === "function") {
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "btn btn-link p-0 text-primary-600 fw-semibold text-sm";
            btn.textContent = actionLabel || "Open";
            btn.addEventListener("click", () => onOpen());
            actionRow.appendChild(btn);
        }

        if (actionRow.childNodes.length) body.appendChild(actionRow);

        toast.appendChild(header);
        toast.appendChild(body);
        host.appendChild(toast);

        return { toast, closeBtn };
    }

    function show(opts) {
        const built = buildToastEl(opts || {});
        if (!built) return;

        const { toast, closeBtn } = built;

        let instance = null;
        if (window.bootstrap?.Toast) {
            instance = window.bootstrap.Toast.getOrCreateInstance(toast);
            instance.show();
            closeBtn.addEventListener("click", () => instance.hide());
            toast.addEventListener("hidden.bs.toast", () => toast.remove());
        } else {
            closeBtn.addEventListener("click", () => toast.remove());
            setTimeout(() => toast.remove(), (opts?.timeoutMs ?? 6500));
        }
    }

    window.SwimsToasts = { show, openNotifDropdown };

    // Init offset calc
    if (document.readyState === "complete" || document.readyState === "interactive") initToastOffset();
    else document.addEventListener("DOMContentLoaded", initToastOffset);

    // Flush queued
    const queued = window.__swimsInitialToasts;
    if (Array.isArray(queued) && queued.length) {
        queued.forEach(t => show(t));
        window.__swimsInitialToasts = [];
    }
})();
