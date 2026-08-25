const ITEM_SELECTOR = ":scope [data-slot='reveal']";
const REDUCED_MOTION_QUERY = "(prefers-reduced-motion: reduce)";

function numberAttribute(element, name, fallback) {
    const parsed = Number.parseFloat(element.getAttribute(name) ?? "");
    return Number.isFinite(parsed) ? parsed : fallback;
}

function isDisabled(item) {
    return item.getAttribute("data-reveal-disabled") === "true";
}

export function attachRevealGroup(root) {
    if (!(root instanceof HTMLElement)) {
        return { refresh() {}, dispose() {} };
    }

    const media = window.matchMedia(REDUCED_MOTION_QUERY);
    const registered = new Set();
    const observedItems = new Map();
    let observer;
    let mutations;
    let disposed = false;
    let layoutFrame;
    let revealFrame;

    const reduced = () => media.matches || root.dataset.revealReducedMotion === "true";
    const paused = () => root.dataset.revealPaused === "true";
    const once = () => root.dataset.revealOnce !== "false";

    const observationTarget = item =>
        item.dataset.revealEffect === "clip" && item.parentElement instanceof HTMLElement
            ? item.parentElement
            : item;

    const stopObserving = item => {
        const target = observationTarget(item);
        const items = observedItems.get(target);
        if (!items) return;
        items.delete(item);
        if (items.size > 0) return;
        observedItems.delete(target);
        observer?.unobserve(target);
    };

    const observe = item => {
        const target = observationTarget(item);
        let items = observedItems.get(target);
        if (!items) {
            items = new Set();
            observedItems.set(target, items);
            observer?.observe(target);
        }
        items.add(item);
    };

    const show = (item, index = 0) => {
        const stagger = Math.min(numberAttribute(root, "data-reveal-stagger", 60) * index, 360);
        item.style.setProperty("--shadcn-reveal-stagger-delay", `${stagger}ms`);
        item.setAttribute("data-reveal-state", "revealed");
        item.removeAttribute("aria-hidden");
        if (once()) stopObserving(item);
    };

    const revealWithoutMotion = () => {
        registered.forEach(item => show(item));
    };

    const onIntersect = entries => {
        if (paused() || reduced()) return;
        let visibleIndex = 0;
        for (const entry of entries) {
            if (entry.isIntersecting) {
                const items = observedItems.get(entry.target) ?? [entry.target];
                for (const item of [...items]) show(item, visibleIndex++);
            } else if (!once()) {
                const items = observedItems.get(entry.target) ?? [entry.target];
                for (const item of items) item.setAttribute("data-reveal-state", "pending");
            }
        }
    };

    const createObserver = () => {
        observer?.disconnect();
        observer = new IntersectionObserver(onIntersect, {
            threshold: numberAttribute(root, "data-reveal-threshold", 0.08),
            rootMargin: root.getAttribute("data-reveal-root-margin") ?? "32px 0px"
        });
        for (const target of observedItems.keys()) observer.observe(target);
    };

    const register = item => {
        if (!(item instanceof HTMLElement) || registered.has(item)) return;
        registered.add(item);

        if (isDisabled(item) || reduced()) {
            show(item);
            return;
        }

        const rect = item.getBoundingClientRect();
        const immediatelyVisible = rect.bottom >= 0 && rect.top <= window.innerHeight + 32;
        if (immediatelyVisible) {
            show(item);
            return;
        }

        item.setAttribute("data-reveal-state", "pending");
        observe(item);
    };

    const refreshNow = () => {
        if (disposed) return;
        if (reduced() || root.dataset.revealDisabled === "true") {
            revealWithoutMotion();
            observer?.disconnect();
            return;
        }

        root.querySelectorAll(ITEM_SELECTOR).forEach(register);
        if (!paused()) {
            registered.forEach(item => {
                if (item.dataset.revealState !== "revealed" && !isDisabled(item)) observe(item);
            });
        }
    };

    const refresh = () => {
        if (disposed) return;
        window.cancelAnimationFrame(layoutFrame);
        window.cancelAnimationFrame(revealFrame);
        layoutFrame = window.requestAnimationFrame(() => {
            revealFrame = window.requestAnimationFrame(refreshNow);
        });
    };

    createObserver();
    refresh();

    mutations = new MutationObserver(records => {
        const requiresObserverRefresh = records.some(record =>
            record.type === "attributes" &&
            (record.attributeName === "data-reveal-threshold" || record.attributeName === "data-reveal-root-margin"));
        if (requiresObserverRefresh) createObserver();
        refresh();
    });
    mutations.observe(root, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: [
            "data-reveal-paused", "data-reveal-reduced-motion", "data-reveal-disabled",
            "data-reveal-threshold", "data-reveal-root-margin"
        ]
    });
    media.addEventListener("change", refresh);

    return {
        refresh,
        dispose() {
            disposed = true;
            window.cancelAnimationFrame(layoutFrame);
            window.cancelAnimationFrame(revealFrame);
            observer?.disconnect();
            observedItems.clear();
            mutations?.disconnect();
            media.removeEventListener("change", refresh);
            registered.clear();
        }
    };
}
