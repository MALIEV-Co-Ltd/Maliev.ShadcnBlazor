export function isDesktopWorkbench() {
    return window.matchMedia("(min-width: 64.0625rem)").matches;
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
