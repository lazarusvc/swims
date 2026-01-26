(function () {
    const HOST_ID = "swims-toast-stack";

    function kindMeta(kind) {
        switch ((kind || "").toLowerCase()) {
            case "success":
                return { dot: "bg-success-600", icon: "bi:patch-check", text: "text-success-600" };
            case "warning":
                return { dot: "bg-warning-600", icon: "mdi:clock-outline", text: "text-warning-600" };
            case "danger":
            case "error":
                return { dot: "bg-danger-600", icon: "mingcute:delete-2-line", text: "text-danger-600" };
            default:
                return { dot: "bg-info-600", icon: "mynaui:check-octagon", text: "text-info-600" };
        }
    }

    function buildToastEl({ title, message, kind, url, urlLabel, timeoutMs }) {
        const host = document.getElementById(HOST_ID);
        if (!host) return null;

        const meta = kindMeta(kind);

        const toast = document.createElement("div");
        toast.className = "toast border-0 shadow-lg radius-12 bg-base mb-12";
        toast.setAttribute("role", "alert");
        toast.setAttribute("aria-live", "assertive");
        toast.setAttribute("aria-atomic", "true");

        // Bootstrap toast options via data attrs
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

        const closeBtn = document.createElement("button");
        closeBtn.type = "button";
        closeBtn.className = `remove-button ${meta.text} text-xxl line-height-1`;
        closeBtn.setAttribute("aria-label", "Close");

        const closeIcon = document.createElement("iconify-icon");
        closeIcon.setAttribute("icon", "iconamoon:sign-times-light");
        closeIcon.className = "icon";

        closeBtn.appendChild(closeIcon);

        header.appendChild(dot);
        header.appendChild(icon);
        header.appendChild(strong);
        header.appendChild(closeBtn);

        const body = document.createElement("div");
        body.className = "toast-body pt-0 pb-12 px-16";

        const msg = document.createElement("div");
        msg.className = "text-secondary-light text-sm";
        msg.textContent = message || "";

        body.appendChild(msg);

        if (url) {
            const a = document.createElement("a");
            a.className = "text-primary-600 fw-semibold text-sm mt-8 d-inline-block";
            a.href = url;
            a.textContent = urlLabel || "Open";
            body.appendChild(a);
        }

        toast.appendChild(header);
        toast.appendChild(body);

        host.appendChild(toast);

        return { toast, closeBtn };
    }

    function show(opts) {
        const built = buildToastEl(opts || {});
        if (!built) return;

        const { toast, closeBtn } = built;

        // Bootstrap is already on the page in WowDash
        let instance = null;
        if (window.bootstrap?.Toast) {
            instance = window.bootstrap.Toast.getOrCreateInstance(toast);
            instance.show();
            closeBtn.addEventListener("click", () => instance.hide());
            toast.addEventListener("hidden.bs.toast", () => toast.remove());
        } else {
            // fallback: remove after delay
            closeBtn.addEventListener("click", () => toast.remove());
            setTimeout(() => toast.remove(), opts?.timeoutMs ?? 6500);
        }
    }

    // Expose
    window.SwimsToasts = { show };

    // Flush any queued toasts (from TempData etc.)
    const queued = window.__swimsInitialToasts;
    if (Array.isArray(queued) && queued.length) {
        queued.forEach(t => show(t));
        window.__swimsInitialToasts = [];
    }
})();
