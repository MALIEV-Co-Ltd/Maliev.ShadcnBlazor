const observers = new Map();
let nextHandle = 1;

export function attachBentoReveal(root) {
    if (!(root instanceof HTMLElement)) return 0;
    const handle = nextHandle++;
    const scroller = root.closest(".theme-preview-region");
    const items = [...root.querySelectorAll("[data-slot='bento-item']")];
    const reduced = root.dataset.reducedMotion === "true";
    if (reduced || !("IntersectionObserver" in window)) {
        items.forEach(item => item.dataset.revealState = "revealed");
        return handle;
    }

    const cleanups = [];
    const reveal = item => {
        item.dataset.revealState = "revealing";
        let timer = 0;
        const finish = event => {
            if (event && (event.target !== item || event.animationName !== "theme-bento-card-reveal")) return;
            item.removeEventListener("animationend", finish);
            window.clearTimeout(timer);
            if (item.dataset.revealState === "revealing") item.dataset.revealState = "revealed";
        };
        item.addEventListener("animationend", finish);
        timer = window.setTimeout(() => finish(), 800);
        cleanups.push(() => {
            item.removeEventListener("animationend", finish);
            window.clearTimeout(timer);
        });
    };
    items.forEach(item => item.dataset.revealState = "pending");
    const observer = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            reveal(entry.target);
            observer.unobserve(entry.target);
        }
    }, { root: scroller instanceof HTMLElement ? scroller : null, threshold: 0.08, rootMargin: "32px 0px" });
    items.forEach(item => observer.observe(item));
    observers.set(handle, { observer, cleanups });
    return handle;
}

export function detachBentoReveal(handle) {
    const entry = observers.get(handle);
    entry?.observer.disconnect();
    entry?.cleanups.forEach(cleanup => cleanup());
    observers.delete(handle);
}
