export function attachRunway(root, dotnet) {
    if (!root) throw new Error("Theme runway root is required.");

    const tracks = [...root.querySelectorAll("[data-runway-track]")];
    const state = {
        lengths: new Map(),
        positions: new Map(),
        last: performance.now(),
        temporary: false,
        persistent: false,
        disposed: false,
        expectedScroll: new WeakMap(),
        resumeTimer: 0
    };
    const speed = 9;
    const reducedQuery = matchMedia("(prefers-reduced-motion: reduce)");
    const onReducedChange = event => dotnet.invokeMethodAsync("SetSystemReducedMotion", event.matches);
    dotnet.invokeMethodAsync("SetSystemReducedMotion", reducedQuery.matches);
    reducedQuery.addEventListener("change", onReducedChange);

    function isStaticLayout() {
        return root.clientWidth < 704 || reducedQuery.matches || root.dataset.reducedMotion === "true";
    }

    function paused() {
        return state.temporary || state.persistent || document.hidden || isStaticLayout();
    }

    function setScrollTop(viewport, value) {
        state.positions.set(viewport, value);
        viewport.scrollTop = value;
        state.expectedScroll.set(viewport, viewport.scrollTop);
    }

    function measure(viewport) {
        const sequence = viewport.querySelector("[data-runway-sequence]");
        const mirror = sequence?.querySelector(".theme-runway__mirror");
        if (!sequence || !mirror) return 0;

        const length = mirror.offsetTop;
        const previousLength = state.lengths.get(viewport);
        state.lengths.set(viewport, length);
        if (!length || isStaticLayout()) return length;

        if (!previousLength) {
            setScrollTop(viewport, viewport.dataset.runwayTrack === "left" ? length : 0);
        }
        return length;
    }

    function normalize(viewport) {
        const length = state.lengths.get(viewport) || measure(viewport);
        if (!length || isStaticLayout()) return;
        const position = state.positions.get(viewport) ?? viewport.scrollTop;
        if (position >= length) setScrollTop(viewport, position - length);
        else if (position <= 0) setScrollTop(viewport, position + length);
    }

    function render(now) {
        if (state.disposed) return;
        const elapsed = Math.min(64, now - state.last);
        state.last = now;

        for (const viewport of tracks) {
            const length = measure(viewport);
            if (!length || paused()) continue;
            const direction = viewport.dataset.runwayTrack === "left" ? -1 : 1;
            const position = state.positions.get(viewport) ?? viewport.scrollTop;
            setScrollTop(viewport, position + direction * elapsed * speed / 1000);
            normalize(viewport);
        }
        requestAnimationFrame(render);
    }

    function setTemporary(value, delayed = false) {
        clearTimeout(state.resumeTimer);
        if (value) {
            state.temporary = true;
            dotnet.invokeMethodAsync("SetInteractionPaused", true);
        } else if (delayed) {
            state.resumeTimer = setTimeout(() => {
                if (root.contains(document.activeElement) || root.matches(":hover")) return;
                state.temporary = false;
                dotnet.invokeMethodAsync("SetInteractionPaused", false);
            }, 1800);
        } else {
            state.temporary = false;
            dotnet.invokeMethodAsync("SetInteractionPaused", false);
        }
    }

    const onEnter = () => setTemporary(true);
    const onLeave = () => setTemporary(false, true);
    const onFocusIn = () => setTemporary(true);
    const onFocusOut = () => queueMicrotask(() => { if (!root.contains(document.activeElement)) setTemporary(false, true); });
    const onActivity = () => { setTemporary(true); setTemporary(false, true); };
    const onScroll = event => {
        const viewport = event.currentTarget;
        const expected = state.expectedScroll.get(viewport);
        if (expected !== undefined && Math.abs(viewport.scrollTop - expected) < 1) return;
        state.expectedScroll.delete(viewport);
        state.positions.set(viewport, viewport.scrollTop);
        normalize(viewport);
        onActivity();
    };
    const onVisibilityChange = () => { for (const viewport of tracks) normalize(viewport); };

    root.addEventListener("pointerenter", onEnter);
    root.addEventListener("pointerleave", onLeave);
    root.addEventListener("focusin", onFocusIn);
    root.addEventListener("focusout", onFocusOut);
    root.addEventListener("pointerdown", onActivity, { passive: true });
    root.addEventListener("wheel", onActivity, { passive: true });
    root.addEventListener("touchstart", onActivity, { passive: true });
    root.addEventListener("keydown", onActivity);
    for (const viewport of tracks) viewport.addEventListener("scroll", onScroll, { passive: true });
    document.addEventListener("visibilitychange", onVisibilityChange);
    for (const viewport of tracks) measure(viewport);
    requestAnimationFrame(render);

    return {
        setPersistentPaused(value) { state.persistent = Boolean(value); state.last = performance.now(); },
        refresh() { for (const viewport of tracks) measure(viewport); },
        dispose() {
            state.disposed = true;
            clearTimeout(state.resumeTimer);
            root.removeEventListener("pointerenter", onEnter);
            root.removeEventListener("pointerleave", onLeave);
            root.removeEventListener("focusin", onFocusIn);
            root.removeEventListener("focusout", onFocusOut);
            root.removeEventListener("pointerdown", onActivity);
            root.removeEventListener("wheel", onActivity);
            root.removeEventListener("touchstart", onActivity);
            root.removeEventListener("keydown", onActivity);
            for (const viewport of tracks) viewport.removeEventListener("scroll", onScroll);
            document.removeEventListener("visibilitychange", onVisibilityChange);
            reducedQuery.removeEventListener("change", onReducedChange);
        }
    };
}
