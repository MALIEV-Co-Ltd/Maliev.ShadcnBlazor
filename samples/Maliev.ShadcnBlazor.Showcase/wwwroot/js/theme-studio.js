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
