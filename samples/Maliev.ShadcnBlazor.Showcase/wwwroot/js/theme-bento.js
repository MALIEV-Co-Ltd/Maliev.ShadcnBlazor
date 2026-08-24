const observers = new Map();
let nextHandle = 1;

export function attachBentoReveal(root) {
    if (!(root instanceof HTMLElement)) return 0;
    const handle = nextHandle++;
    const scroller = root.closest(".theme-preview-region");
    const items = [...root.querySelectorAll("[data-slot='bento-item']")];
    const reduced = root.dataset.reducedMotion === "true";
    if (reduced || !("IntersectionObserver" in window)) {
        items.forEach(item => item.dataset.revealState = "visible");
        return handle;
    }

    items.forEach(item => item.dataset.revealState = "pending");
    const observer = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (!entry.isIntersecting) continue;
            entry.target.dataset.revealState = "visible";
            observer.unobserve(entry.target);
        }
    }, { root: scroller instanceof HTMLElement ? scroller : null, threshold: 0.08, rootMargin: "32px 0px" });
    items.forEach(item => observer.observe(item));
    observers.set(handle, observer);
    return handle;
}

export function detachBentoReveal(handle) {
    const observer = observers.get(handle);
    observer?.disconnect();
    observers.delete(handle);
}
