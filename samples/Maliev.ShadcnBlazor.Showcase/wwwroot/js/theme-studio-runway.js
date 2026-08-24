export function attachRunway(root, dotnet) {
    if (!root) throw new Error("Theme runway root is required.");
    const tracks = [...root.querySelectorAll("[data-runway-track]")];
    const state = {
        progress: new Map(tracks.map(track => [track, 0])),
        lengths: new Map(),
        transforms: new Map(),
        last: performance.now(),
        temporary: false,
        persistent: false,
        disposed: false,
        resumeTimer: 0
    };
    const speed = 9;
    const reducedQuery = matchMedia("(prefers-reduced-motion: reduce)");
    const onReducedChange = event => dotnet.invokeMethodAsync("SetSystemReducedMotion", event.matches);
    dotnet.invokeMethodAsync("SetSystemReducedMotion", reducedQuery.matches);
    reducedQuery.addEventListener("change", onReducedChange);

    function isStaticLayout() {
        return root.clientWidth < 704 || matchMedia("(prefers-reduced-motion: reduce)").matches || root.dataset.reducedMotion === "true";
    }
    function paused() { return state.temporary || state.persistent || document.hidden || isStaticLayout(); }
    function render() {
        for (const viewport of tracks) {
            const sequence = viewport.querySelector("[data-runway-sequence]");
            const mirror = sequence?.querySelector(".theme-runway__mirror");
            if (!sequence || !mirror) continue;
            const length = mirror.offsetTop;
            if (!length || isStaticLayout()) { sequence.style.transform = ""; state.transforms.delete(sequence); continue; }
            let progress = state.progress.get(viewport) ?? 0;
            const previousLength = state.lengths.get(viewport);
            if (previousLength && Math.abs(previousLength - length) > 0.5) {
                const renderedTransform = sequence.style.transform || state.transforms.get(sequence) || getComputedStyle(sequence).transform;
                const renderedOffset = renderedTransform === "none" ? 0 : new DOMMatrix(renderedTransform).m42;
                progress = viewport.dataset.runwayTrack === "left" ? renderedOffset + length : -renderedOffset;
                state.progress.set(viewport, progress);
            }
            state.lengths.set(viewport, length);
            const normalized = ((progress % length) + length) % length;
            const transform = viewport.dataset.runwayTrack === "left"
                ? `translate3d(0, ${normalized - length}px, 0)`
                : `translate3d(0, ${-normalized}px, 0)`;
            sequence.style.transform = transform;
            state.transforms.set(sequence, transform);
        }
    }
    function frame(now) {
        if (state.disposed) return;
        const elapsed = Math.min(64, now - state.last);
        state.last = now;
        if (!paused()) {
            for (const viewport of tracks)
                state.progress.set(viewport, (state.progress.get(viewport) ?? 0) + elapsed * speed / 1000);
        }
        render();
        requestAnimationFrame(frame);
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
    root.addEventListener("pointerenter", onEnter);
    root.addEventListener("pointerleave", onLeave);
    root.addEventListener("focusin", onFocusIn);
    root.addEventListener("focusout", onFocusOut);
    root.addEventListener("pointerdown", onActivity, { passive: true });
    root.addEventListener("wheel", onActivity, { passive: true });
    root.addEventListener("touchstart", onActivity, { passive: true });
    root.addEventListener("keydown", onActivity);
    document.addEventListener("visibilitychange", render);
    requestAnimationFrame(frame);

    return {
        setPersistentPaused(value) { state.persistent = Boolean(value); state.last = performance.now(); },
        refresh() { render(); },
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
            document.removeEventListener("visibilitychange", render);
            reducedQuery.removeEventListener("change", onReducedChange);
        }
    };
}
