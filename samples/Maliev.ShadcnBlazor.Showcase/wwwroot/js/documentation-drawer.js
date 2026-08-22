const attached = new WeakMap();
const focusableSelector = [
    'a[href]',
    'button:not([disabled])',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])'
].join(',');

function visibleFocusable(drawer) {
    return [...drawer.querySelectorAll(focusableSelector)].filter(element =>
        !element.hidden && element.getAttribute('aria-hidden') !== 'true' && element.getClientRects().length > 0);
}

export function attach(drawer) {
    if (!drawer || attached.has(drawer)) return;
    const onKeyDown = event => {
        if (event.key !== 'Tab') return;
        const focusable = visibleFocusable(drawer);
        if (focusable.length === 0) {
            event.preventDefault();
            drawer.focus();
            return;
        }
        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    };
    drawer.addEventListener('keydown', onKeyDown);
    const state = { onKeyDown, focusFrame: 0 };
    const focusWhenVisible = attempt => {
        const focusTarget = drawer.querySelector('[data-testid="outline-close"]') ?? visibleFocusable(drawer)[0];
        if (focusTarget && getComputedStyle(focusTarget).visibility !== 'visible' && attempt < 4) {
            state.focusFrame = requestAnimationFrame(() => focusWhenVisible(attempt + 1));
            return;
        }
        focusTarget?.focus({ preventScroll: true });
        drawer.dataset.drawerFocusReady = document.activeElement === focusTarget ? 'true' : 'false';
    };
    state.focusFrame = requestAnimationFrame(() => focusWhenVisible(0));
    drawer.dataset.drawerReady = 'true';
    attached.set(drawer, state);
}

export function detach(drawer) {
    const state = drawer && attached.get(drawer);
    if (!state) return;
    cancelAnimationFrame(state.focusFrame);
    drawer.removeEventListener('keydown', state.onKeyDown);
    delete drawer.dataset.drawerReady;
    delete drawer.dataset.drawerFocusReady;
    attached.delete(drawer);
}
