const delayedTriggers = new WeakMap();

function isWithinContent(contentId, target) {
    const content = document.getElementById(contentId);
    return Boolean(content && target instanceof Node && content.contains(target));
}

export function attachDelayedTrigger(trigger, dotnet, openDelay, closeDelay, contentId) {
    detachDelayedTrigger(trigger);
    let timer = 0;
    let suppressionTimer = 0;
    let suppressFocusOpen = false;
    const schedule = (open, delay) => {
        clearTimeout(timer);
        timer = setTimeout(() => dotnet.invokeMethodAsync("RequestOpenAsync", open), delay);
    };
    const enter = () => schedule(true, openDelay);
    const leave = event => {
        if (isWithinContent(contentId, event.relatedTarget)) return;
        schedule(false, closeDelay);
    };
    const focusIn = () => {
        if (suppressFocusOpen) {
            suppressFocusOpen = false;
            clearTimeout(suppressionTimer);
            return;
        }
        schedule(true, openDelay);
    };
    const focusOut = event => {
        if (trigger.contains(event.relatedTarget) || isWithinContent(contentId, event.relatedTarget)) return;
        schedule(false, closeDelay);
    };
    const keydown = event => {
        if (event.key !== "Escape" || !document.getElementById(contentId)) return;
        suppressFocusOpen = true;
        clearTimeout(suppressionTimer);
        suppressionTimer = setTimeout(() => { suppressFocusOpen = false; }, Math.max(closeDelay, openDelay, 100));
    };

    trigger.addEventListener("pointerenter", enter);
    trigger.addEventListener("pointerleave", leave);
    trigger.addEventListener("focusin", focusIn);
    trigger.addEventListener("focusout", focusOut);
    document.addEventListener("keydown", keydown, true);
    delayedTriggers.set(trigger, {
        enter,
        leave,
        focusIn,
        focusOut,
        keydown,
        clear: () => { clearTimeout(timer); clearTimeout(suppressionTimer); }
    });
}

export function detachDelayedTrigger(trigger) {
    const value = delayedTriggers.get(trigger);
    if (!value) return;
    value.clear();
    trigger.removeEventListener("pointerenter", value.enter);
    trigger.removeEventListener("pointerleave", value.leave);
    trigger.removeEventListener("focusin", value.focusIn);
    trigger.removeEventListener("focusout", value.focusOut);
    document.removeEventListener("keydown", value.keydown, true);
    delayedTriggers.delete(trigger);
}
