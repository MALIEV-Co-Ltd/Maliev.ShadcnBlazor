export function isDesktopWorkbench() {
    return window.matchMedia("(min-width: 64.0625rem)").matches;
}

export function resetInitialPreviewScroll() {
    return new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(() => {
        const preview = document.querySelector(".theme-preview-region");
        if (preview instanceof HTMLElement) {
            preview.scrollTop = 0;
            preview.scrollLeft = 0;
        }
        if (document.scrollingElement) document.scrollingElement.scrollTop = 0;
        document.body.scrollTop = 0;
        document.documentElement.scrollTop = 0;
        resolve();
    })));
}

export function capturePreviewScroll() {
    const preview = document.querySelector(".theme-preview-region");
    return preview instanceof HTMLElement ? { top: preview.scrollTop, left: preview.scrollLeft } : { top: 0, left: 0 };
}

export function restorePreviewScroll(position) {
    return new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(() => {
        const preview = document.querySelector(".theme-preview-region");
        if (preview instanceof HTMLElement) {
            preview.scrollTop = Number(position?.top) || 0;
            preview.scrollLeft = Number(position?.left) || 0;
        }
        resolve();
    })));
}

export function preservePreviewScrollOnInput(root) {
    const input = root instanceof HTMLInputElement ? root : root?.querySelector?.("input");
    if (!(input instanceof HTMLInputElement)) return { dispose() {} };
    const preview = input.closest(".theme-preview-region");
    if (!(preview instanceof HTMLElement)) return { dispose() {} };
    let top = preview.scrollTop;
    let left = preview.scrollLeft;
    let editing = false;
    const capture = () => { top = preview.scrollTop; left = preview.scrollLeft; };
    const restore = () => {
        preview.scrollTop = top;
        preview.scrollLeft = left;
        requestAnimationFrame(() => {
            preview.scrollTop = top;
            preview.scrollLeft = left;
        });
    };
    const begin = () => { editing = true; capture(); };
    const end = () => { editing = false; };
    const beforeInput = () => { if (!editing) capture(); };
    input.addEventListener("focus", begin);
    input.addEventListener("blur", end);
    input.addEventListener("beforeinput", beforeInput);
    input.addEventListener("input", restore);
    return {
        dispose() {
            input.removeEventListener("focus", begin);
            input.removeEventListener("blur", end);
            input.removeEventListener("beforeinput", beforeInput);
            input.removeEventListener("input", restore);
        }
    };
}

export function loadGoogleFonts(stylesheet, timeoutMs = 5000) {
    const id = "theme-studio-google-fonts";
    const existing = document.getElementById(id);
    if (!stylesheet) {
        existing?.remove();
        return Promise.resolve("bundled");
    }

    let url;
    try {
        url = new URL(stylesheet);
    } catch {
        return Promise.resolve("failed");
    }
    if (url.protocol !== "https:" || url.hostname !== "fonts.googleapis.com" || url.pathname !== "/css2")
        return Promise.resolve("failed");
    if (existing instanceof HTMLLinkElement && existing.href === url.href && existing.dataset.state === "loaded")
        return Promise.resolve("loaded");
    existing?.remove();

    const link = document.createElement("link");
    link.id = id;
    link.rel = "stylesheet";
    link.href = url.href;
    link.dataset.state = "loading";
    document.head.append(link);
    return new Promise(resolve => {
        let settled = false;
        let timer;
        const finish = state => {
            if (settled) return;
            settled = true;
            window.clearTimeout(timer);
            link.dataset.state = state;
            link.onload = null;
            link.onerror = null;
            resolve(state);
        };
        link.onload = () => finish("loaded");
        link.onerror = () => finish("failed");
        timer = window.setTimeout(() => finish("timeout"), Math.max(1000, Math.min(Number(timeoutMs) || 5000, 10000)));
    });
}

export function bindPaletteWorkbench(root, returnFocusId) {
    if (!(root instanceof HTMLElement)) return { restoreFocus() {}, dispose() {} };
    const media = window.matchMedia("(max-width: 64rem)");
    const focusableSelector = [
        "a[href]",
        "button:not([disabled])",
        "input:not([disabled])",
        "textarea:not([disabled])",
        "select:not([disabled])",
        "[tabindex]:not([tabindex='-1'])"
    ].join(",");

    const focusable = () => [...root.querySelectorAll(focusableSelector)]
        .filter(element => element instanceof HTMLElement && element.getClientRects().length > 0 && element.getAttribute("aria-hidden") !== "true");
    const focusFirst = () => requestAnimationFrame(() => focusable()[0]?.focus({ preventScroll: true }));
    let concealedBackground = [];
    const concealBackground = () => {
        if (concealedBackground.length) return;
        const element = root.parentElement?.querySelector(":scope > .theme-studio-sidebar-provider");
        if (!(element instanceof HTMLElement)) return;
        concealedBackground.push({
            element,
            inert: element.inert,
            ariaHidden: element.getAttribute("aria-hidden")
        });
        element.inert = true;
        element.setAttribute("aria-hidden", "true");
    };
    const revealBackground = restorePrevious => {
        for (const entry of concealedBackground) {
            if (restorePrevious) {
                entry.element.inert = entry.inert;
                if (entry.ariaHidden === null) entry.element.removeAttribute("aria-hidden");
                else entry.element.setAttribute("aria-hidden", entry.ariaHidden);
            } else {
                entry.element.inert = false;
                entry.element.removeAttribute("aria-hidden");
            }
        }
        concealedBackground = [];
    };
    const applyMode = () => {
        if (media.matches) {
            root.inert = false;
            root.removeAttribute("aria-hidden");
            concealBackground();
            root.setAttribute("role", "dialog");
            root.setAttribute("aria-modal", "true");
            focusFirst();
        } else {
            revealBackground(false);
            root.inert = false;
            root.removeAttribute("aria-hidden");
            root.removeAttribute("role");
            root.removeAttribute("aria-modal");
        }
    };
    let modeFrame = 0;
    const onModeChange = () => {
        window.cancelAnimationFrame(modeFrame);
        modeFrame = window.requestAnimationFrame(applyMode);
    };
    const rootObserver = new MutationObserver(() => {
        if (!media.matches || (!root.inert && root.getAttribute("aria-hidden") !== "true")) return;
        root.inert = false;
        root.removeAttribute("aria-hidden");
    });
    const hasActiveListbox = () => [...root.querySelectorAll('[role="listbox"]')]
        .some(listbox => listbox instanceof HTMLElement && listbox.getClientRects().length > 0);
    const isEditableTarget = target => target instanceof Element && Boolean(target.closest(
        "input, textarea, select, button, [contenteditable]:not([contenteditable='false'])"));
    const onKeyDown = event => {
        if (event.key === "Escape") {
            event.preventDefault();
            event.stopPropagation();
            root.querySelector("[data-testid='theme-palette-close']")?.click();
            return;
        }

        if ((event.key === " " || event.key === "Spacebar") && !isEditableTarget(event.target) && !hasActiveListbox()) {
            event.preventDefault();
            event.stopPropagation();
            root.querySelector("[data-testid='theme-palette-generate']")?.click();
            return;
        }

        if (event.key !== "Tab" || !media.matches) return;
        const controls = focusable();
        if (!controls.length) {
            event.preventDefault();
            root.focus({ preventScroll: true });
            return;
        }
        const first = controls[0];
        const last = controls[controls.length - 1];
        if (event.shiftKey && (document.activeElement === first || document.activeElement === root)) {
            event.preventDefault();
            last.focus({ preventScroll: true });
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus({ preventScroll: true });
        }
    };
    const restoreFocus = () => {
        revealBackground(true);
        document.getElementById(returnFocusId)?.focus({ preventScroll: true });
    };
    const dispose = () => {
        revealBackground(true);
        window.cancelAnimationFrame(modeFrame);
        rootObserver.disconnect();
        root.removeEventListener("keydown", onKeyDown);
        media.removeEventListener("change", onModeChange);
        root.removeAttribute("role");
        root.removeAttribute("aria-modal");
    };

    root.addEventListener("keydown", onKeyDown);
    rootObserver.observe(root, { attributes: true, attributeFilter: ["inert", "aria-hidden"] });
    media.addEventListener("change", onModeChange);
    applyMode();
    return { restoreFocus, dispose };
}
