export function focusById(id) {
    document.getElementById(id)?.focus({ preventScroll: true });
}
export function focusFirstInId(id) {
    document.getElementById(id)?.querySelector('a[href],button:not(:disabled),input:not(:disabled),[tabindex]:not([tabindex="-1"])')?.focus({ preventScroll: true });
}

const navigationViewports = new WeakMap();
export function attachNavigationViewport(position) {
    const sync = () => {
        const nav = position.closest('[data-slot="navigation-menu"]');
        const active = nav?.querySelector('[data-slot="navigation-menu-trigger"][data-state="open"]');
        if (!nav || !active) { position.removeAttribute('data-side'); if (position.matches(':popover-open')) position.hidePopover(); return; }
        if (!position.matches(':popover-open')) position.showPopover();
        const navRect = nav.getBoundingClientRect(), triggerRect = active.getBoundingClientRect();
        const viewportRect = position.getBoundingClientRect();
        const spaceBelow = window.innerHeight - triggerRect.bottom - 8, spaceAbove = triggerRect.top - 8;
        const side = viewportRect.height > spaceBelow && spaceAbove > spaceBelow ? 'top' : 'bottom';
        const availableWidth = Math.max(0, window.innerWidth - 16), viewportWidth = Math.min(viewportRect.width, availableWidth), screenLeft = Math.min(Math.max(8, triggerRect.left), Math.max(8, window.innerWidth - 8 - viewportWidth));
        position.dataset.side = side;
        position.style.setProperty('--shadcn-navigation-menu-portal-left', `${screenLeft}px`);
        position.style.setProperty('--shadcn-navigation-menu-portal-top', `${side === 'top' ? Math.max(8, triggerRect.top - viewportRect.height - 8) : triggerRect.bottom + 8}px`);
        position.style.setProperty('--shadcn-navigation-menu-anchor-start', `${screenLeft - navRect.left}px`);
        position.style.setProperty('--shadcn-navigation-menu-available-width', `${availableWidth}px`);
    };
    const observer = new ResizeObserver(sync); observer.observe(position); const mutations = new MutationObserver(sync); mutations.observe(position.closest('[data-slot="navigation-menu"]'), { subtree: true, attributes: true, attributeFilter: ['data-state'] }); window.addEventListener('resize', sync); window.addEventListener('scroll', sync, true); sync();
    navigationViewports.set(position, { observer, mutations, sync });
}
export function detachNavigationViewport(position) { const value = navigationViewports.get(position); if (!value) return; if (position.matches(':popover-open')) position.hidePopover(); value.observer.disconnect(); value.mutations.disconnect(); window.removeEventListener('resize', value.sync); window.removeEventListener('scroll', value.sync, true); navigationViewports.delete(position); }

const keyGuards = new WeakMap();
export function attachKeyGuard(root, keys) {
    const allowed = new Set(keys);
    const keydown = event => { if (allowed.has(event.key)) event.preventDefault(); };
    root.addEventListener('keydown', keydown);
    keyGuards.set(root, keydown);
}
export function detachKeyGuard(root) {
    const keydown = keyGuards.get(root);
    if (!keydown) return;
    root.removeEventListener('keydown', keydown);
    keyGuards.delete(root);
}

const resizable = new WeakMap();
export function attachResizableHandle(handle, dotnet, direction, rtl) {
    let start = null;
    let pending = 0;
    let frame = 0;
    const flush = () => { frame = 0; const delta = pending; pending = 0; if (delta) dotnet.invokeMethodAsync('ResizeFromPointerByPanelIdAsync', handle.dataset.leftPanelId, delta); };
    const move = event => {
        if (!start || event.pointerId !== start.id) return;
        const group = handle.closest('[data-slot="resizable-group"]');
        const rect = group?.getBoundingClientRect();
        const span = direction === 'horizontal' ? rect?.width : rect?.height;
        if (!span) return;
        const physical = direction === 'horizontal' ? event.clientX - start.x : event.clientY - start.y;
        pending += (direction === 'horizontal' && rtl ? -physical : physical) / span * 100;
        start = { id: event.pointerId, x: event.clientX, y: event.clientY };
        if (!frame) frame = requestAnimationFrame(flush);
    };
    const down = event => { if (handle.dataset.disabled === 'true') return; start = { id: event.pointerId, x: event.clientX, y: event.clientY }; handle.setPointerCapture?.(event.pointerId); };
    const up = event => { if (start?.id === event.pointerId) { if (frame) { cancelAnimationFrame(frame); flush(); } start = null; handle.releasePointerCapture?.(event.pointerId); } };
    handle.addEventListener('pointerdown', down); handle.addEventListener('pointermove', move); handle.addEventListener('pointerup', up); handle.addEventListener('pointercancel', up);
    resizable.set(handle, { down, move, up });
}
export function detachResizableHandle(handle) { const value = resizable.get(handle); if (!value) return; handle.removeEventListener('pointerdown', value.down); handle.removeEventListener('pointermove', value.move); handle.removeEventListener('pointerup', value.up); handle.removeEventListener('pointercancel', value.up); resizable.delete(handle); }

const scrollAreas = new WeakMap();
let rtlScrollType;
function detectRtlScrollType() {
    if (rtlScrollType) return rtlScrollType;
    const probe = document.createElement('div'), content = document.createElement('div');
    probe.dir = 'rtl'; probe.style.cssText = 'position:absolute;left:-9999px;width:4px;height:1px;overflow:scroll'; content.style.width = '8px'; probe.append(content); document.body.append(probe);
    if (probe.scrollLeft > 0) rtlScrollType = 'default'; else { probe.scrollLeft = 1; rtlScrollType = probe.scrollLeft === 0 ? 'negative' : 'reverse'; }
    probe.remove(); return rtlScrollType;
}
function normalizedScrollLeft(viewport, rtl, max) { if (!rtl) return viewport.scrollLeft; const type = detectRtlScrollType(); return type === 'negative' ? -viewport.scrollLeft : type === 'reverse' ? viewport.scrollLeft : max - viewport.scrollLeft; }
function setNormalizedScrollLeft(viewport, rtl, max, value) { if (!rtl) { viewport.scrollLeft = value; return; } const type = detectRtlScrollType(); viewport.scrollLeft = type === 'negative' ? -value : type === 'reverse' ? value : max - value; }
export function syncScrollArea(root, hideDelay) {
    const viewport = root.querySelector('[data-slot="scroll-area-viewport"]');
    if (!viewport) return;
    const sync = () => {
        const xRatio = viewport.clientWidth / Math.max(viewport.scrollWidth, 1), yRatio = viewport.clientHeight / Math.max(viewport.scrollHeight, 1);
        const maxX = Math.max(0, viewport.scrollWidth - viewport.clientWidth), maxY = Math.max(0, viewport.scrollHeight - viewport.clientHeight);
        const rtl = getComputedStyle(viewport).direction === 'rtl'; const normalizedX = normalizedScrollLeft(viewport, rtl, maxX);
        root.style.setProperty('--shadcn-scroll-area-x-ratio', String(xRatio)); root.style.setProperty('--shadcn-scroll-area-y-ratio', String(yRatio));
        root.style.setProperty('--shadcn-scroll-area-x-position', String(maxX ? normalizedX / maxX : 0)); root.style.setProperty('--shadcn-scroll-area-y-position', String(maxY ? viewport.scrollTop / maxY : 0));
        root.setAttribute('data-overflow-x', viewport.scrollWidth > viewport.clientWidth ? 'true' : 'false'); root.setAttribute('data-overflow-y', viewport.scrollHeight > viewport.clientHeight ? 'true' : 'false');
        root.dataset.scrolling = 'true'; clearTimeout(root.__shadcnScrollTimer); root.__shadcnScrollTimer = setTimeout(() => delete root.dataset.scrolling, hideDelay);
    };
    const pointerdown = event => { const bar = event.target.closest('[data-slot="scroll-area-scrollbar"]'); if (!bar) return; const horizontal = bar.dataset.orientation === 'horizontal', thumb = bar.querySelector('[data-slot="scroll-area-thumb"]'); const rect = bar.getBoundingClientRect(), thumbRect = thumb?.getBoundingClientRect(); const pointerStart = horizontal ? event.clientX : event.clientY, thumbStart = horizontal ? thumbRect?.left : thumbRect?.top; const grabOffset = event.target.closest('[data-slot="scroll-area-thumb"]') && thumbStart != null ? pointerStart - thumbStart : (horizontal ? thumbRect?.width : thumbRect?.height) / 2 || 0; const trackStart = horizontal ? rect.left : rect.top, trackLength = horizontal ? rect.width : rect.height, thumbLength = horizontal ? thumbRect?.width : thumbRect?.height; const update = pointer => { const coordinate = horizontal ? pointer.clientX : pointer.clientY, ratio = (coordinate - trackStart - grabOffset) / Math.max(1, trackLength - (thumbLength || 0)), clamped = Math.max(0, Math.min(1, ratio)); if (horizontal) { const max = viewport.scrollWidth - viewport.clientWidth; setNormalizedScrollLeft(viewport, getComputedStyle(viewport).direction === 'rtl', max, clamped * max); } else viewport.scrollTop = clamped * (viewport.scrollHeight - viewport.clientHeight); }; bar.setPointerCapture?.(event.pointerId); update(event); const move = e => { if (e.pointerId === event.pointerId) update(e); }; const up = e => { if (e.pointerId !== event.pointerId) return; bar.releasePointerCapture?.(e.pointerId); bar.removeEventListener('pointermove', move); bar.removeEventListener('pointerup', up); bar.removeEventListener('pointercancel', up); }; bar.addEventListener('pointermove', move); bar.addEventListener('pointerup', up); bar.addEventListener('pointercancel', up); };
    const content = viewport.querySelector('[data-slot="scroll-area-content"]'); root.addEventListener('pointerdown', pointerdown); viewport.addEventListener('scroll', sync, { passive: true }); const observer = new ResizeObserver(sync); observer.observe(viewport); if (content) observer.observe(content); const mutations = new MutationObserver(sync); if (content) mutations.observe(content, { childList: true, subtree: true, characterData: true }); sync(); scrollAreas.set(root, { viewport, sync, observer, mutations, pointerdown });
}
export function detachScrollArea(root) { const value = scrollAreas.get(root); if (!value) return; root.removeEventListener('pointerdown', value.pointerdown); value.viewport.removeEventListener('scroll', value.sync); value.observer.disconnect(); value.mutations.disconnect(); clearTimeout(root.__shadcnScrollTimer); scrollAreas.delete(root); }

const sidebarProviders = new WeakMap();
export function attachSidebarProvider(root, dotnet, shortcut) {
    const query = matchMedia('(max-width: 48rem)');
    const change = () => dotnet.invokeMethodAsync('SetMobileAsync', query.matches);
    const keydown = event => { if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === shortcut.toLowerCase()) { event.preventDefault(); dotnet.invokeMethodAsync('ToggleFromShortcutAsync'); } };
    query.addEventListener('change', change); document.addEventListener('keydown', keydown); change(); sidebarProviders.set(root, { query, change, keydown });
}
export function detachSidebarProvider(root) { const value = sidebarProviders.get(root); if (!value) return; value.query.removeEventListener('change', value.change); document.removeEventListener('keydown', value.keydown); sidebarProviders.delete(root); }

const sidebarOverlays = new WeakMap();
function releaseSidebarOverlay(aside, restoreFocus = true) { const value = sidebarOverlays.get(aside); if (!value) return; aside.removeEventListener('keydown', value.keydown); value.inerted.forEach(({ element, inert, ariaHidden }) => { element.inert = inert; if (ariaHidden === null) element.removeAttribute('aria-hidden'); else element.setAttribute('aria-hidden', ariaHidden); }); if (restoreFocus) value.previous?.focus?.({ preventScroll: true }); sidebarOverlays.delete(aside); }
export function attachSidebarOverlay(aside, dotnet) {
    const previous = document.activeElement;
    const inerted = []; let branch = aside;
    while (branch && branch !== document.body) { const parent = branch.parentElement; if (!parent) break; [...parent.children].filter(element => element !== branch && element.dataset.slot !== 'sidebar-backdrop').forEach(element => inerted.push({ element, inert: element.inert, ariaHidden: element.getAttribute('aria-hidden') })); branch = parent; }
    inerted.forEach(({ element }) => { element.inert = true; element.setAttribute('aria-hidden', 'true'); });
    const focusable = () => [...aside.querySelectorAll('a[href],button:not(:disabled),input:not(:disabled),[tabindex]:not([tabindex="-1"])')];
    const keydown = event => {
        if (event.key === 'Escape') { event.preventDefault(); releaseSidebarOverlay(aside); dotnet.invokeMethodAsync('CloseMobileFromOverlayAsync'); return; }
        if (event.key !== 'Tab') return;
        const items = focusable(); if (!items.length) { event.preventDefault(); aside.focus(); return; }
        const first = items[0], last = items[items.length - 1];
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
    };
    aside.addEventListener('keydown', keydown); queueMicrotask(() => (focusable()[0] || aside).focus()); sidebarOverlays.set(aside, { keydown, previous, inerted });
}
export function detachSidebarOverlay(aside) { releaseSidebarOverlay(aside); }
