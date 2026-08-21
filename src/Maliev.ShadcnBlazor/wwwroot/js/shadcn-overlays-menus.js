const dialogs = new WeakMap();
const dialogStack = [];
const layerStack = [];
const layerRefs = new WeakMap();
function acquireLayer(element) { const refs=layerRefs.get(element)||0;layerRefs.set(element,refs+1);if(!refs)layerStack.push(element); }
function releaseLayer(element) { const refs=layerRefs.get(element)||0;if(refs<=1){layerRefs.delete(element);const index=layerStack.indexOf(element);if(index>=0)layerStack.splice(index,1);}else layerRefs.set(element,refs-1); }
function isTopLayer(element) { return layerStack[layerStack.length-1]===element; }
const inertOwners = new WeakMap();
let modalCount = 0;
let documentOverflow = '';

function focusable(root) {
    return [...root.querySelectorAll('a[href],button:not(:disabled),input:not(:disabled),select:not(:disabled),textarea:not(:disabled),[tabindex]:not([tabindex="-1"])')]
        .filter(element => !element.hidden && element.getAttribute('aria-hidden') !== 'true');
}

function releaseDialog(content, restore = true) {
    const state = dialogs.get(content);
    if (!state) return;
    document.removeEventListener('keydown', state.keydown);
    releaseLayer(content);
    state.observer?.disconnect();
    state.inerted.forEach(element => {
        const ownership = inertOwners.get(element);
        if (!ownership) return;
        ownership.count--;
        if (ownership.count > 0) return;
        element.inert = ownership.inert;
        if (ownership.ariaHidden === null) element.removeAttribute('aria-hidden'); else element.setAttribute('aria-hidden', ownership.ariaHidden);
        inertOwners.delete(element);
    });
    const stackIndex = dialogStack.indexOf(state);
    const wasTopmost = stackIndex === dialogStack.length - 1;
    if (stackIndex >= 0) dialogStack.splice(stackIndex, 1);
    if (state.modal && --modalCount === 0) document.documentElement.style.overflow = documentOverflow;
    if (content.matches(':popover-open')) content.hidePopover();
    if (restore && wasTopmost) {
        const restoreFocus = () => {
            const target = state.previous?.isConnected
                ? state.previous
                : state.focusOwner?.querySelector?.('[data-slot="dialog-trigger"]');
            target?.focus?.({ preventScroll: true });
        };
        restoreFocus();
        requestAnimationFrame(restoreFocus);
    }
    dialogs.delete(content);
}

export function attachDialog(content, dotnet, modal, closeOnEscape, trapFocus = modal) {
    if (dialogs.has(content)) return;
    const previous = document.activeElement;
    const focusOwner = previous?.closest?.('[data-slot="dialog"]');
    const portal = content.closest('[data-slot$="-portal"]');
    if (content.showPopover) { content.setAttribute('popover', 'manual'); content.showPopover(); }
    const inerted = new Set();
    if (modal) {
        let branch = portal || content;
        while (branch && branch !== document.body) {
            const parent = branch.parentElement;
            if (!parent) break;
            [...parent.children].filter(element => element !== branch).forEach(element => inerted.add(element));
            branch = parent;
        }
        inerted.forEach(element => {
            const ownership = inertOwners.get(element);
            if (ownership) ownership.count++;
            else inertOwners.set(element, { count: 1, inert: element.inert, ariaHidden: element.getAttribute('aria-hidden') });
            element.inert = true;
            element.setAttribute('aria-hidden', 'true');
        });
        if (modalCount++ === 0) documentOverflow = document.documentElement.style.overflow;
        document.documentElement.style.overflow = 'hidden';
    }
    const state = { content, previous, focusOwner, inerted: [...inerted], modal, keydown: null, observer: null };
    const keydown = event => {
        if (event.__shadcnLayerHandled || dialogStack[dialogStack.length - 1] !== state || !isTopLayer(content)) return;
        if (event.key === 'Escape' && closeOnEscape) { event.__shadcnLayerHandled=true;event.preventDefault(); event.stopImmediatePropagation(); releaseDialog(content); dotnet.invokeMethodAsync('RequestCloseAsync'); return; }
        if (!trapFocus || event.key !== 'Tab') return;
        const items = focusable(content);
        if (!items.length) { event.preventDefault(); content.focus(); return; }
        const first = items[0], last = items[items.length - 1];
        if (event.shiftKey && (document.activeElement === first || document.activeElement === content)) { event.preventDefault(); last.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
    };
    document.addEventListener('keydown', keydown);
    const observer = new MutationObserver(() => { if (!content.isConnected) releaseDialog(content); });
    state.keydown = keydown;
    state.observer = observer;
    dialogs.set(content, state);
    dialogStack.push(state);
    acquireLayer(content);
    observer.observe(document.body, { childList: true, subtree: true });
    queueMicrotask(() => (content.querySelector('[data-slot="alert-dialog-cancel"]') || content.querySelector('[autofocus]') || focusable(content)[0] || content).focus({ preventScroll: true }));
}

export function detachDialog(content) { releaseDialog(content); }
export function isDialogAttached(content) { return dialogs.has(content); }

const drawers = new WeakMap();
export function attachDrawer(content, dotnet, direction, modalMode, disablePointerDismissal, snapPoints) {
    const modal = modalMode === 'Modal';
    const trap = modal || modalMode === 'TrapFocus';
    attachDialog(content, dotnet, modal, true, trap);
    let drag = null;
    const axis = direction === 'up' || direction === 'down' ? 'y' : 'x';
    const sign = direction === 'down' || direction === 'right' ? 1 : -1;
    const resolveSnapPoint = point => {
        if (typeof point !== 'string') return Number.NaN;
        const value = Number.parseFloat(point);
        if (!Number.isFinite(value) || value <= 0) return Number.NaN;
        if (point.endsWith('px')) return value;
        if (point.endsWith('rem')) return value * Number.parseFloat(getComputedStyle(document.documentElement).fontSize);
        return value <= 1 ? value * innerHeight : Number.NaN;
    };
    const resolvedSnapPoints = () => (snapPoints || []).map((point, index) => ({ point, index, pixels: resolveSnapPoint(point) })).filter(value => Number.isFinite(value.pixels));
    const initialResolved = resolvedSnapPoints();
    if (initialResolved.length !== (snapPoints || []).length || initialResolved.some((value, index) => index > 0 && initialResolved[index - 1].pixels >= value.pixels))
        throw new RangeError('Drawer snap points must resolve to strictly increasing pixel sizes.');
    const applyActiveSnap = () => {
        const active = content.dataset.snapPoint;
        if (!active || axis !== 'y') { content.style.removeProperty('--shadcn-drawer-snap-size'); return; }
        const pixels = resolveSnapPoint(active);
        if (Number.isFinite(pixels)) content.style.setProperty('--shadcn-drawer-snap-size', `${Math.min(innerHeight, pixels)}px`);
    };
    applyActiveSnap();
    const snapObserver = new MutationObserver(applyActiveSnap);
    snapObserver.observe(content, { attributes: true, attributeFilter: ['data-snap-point'] });
    const down = event => {
        if (event.button !== 0 || event.target.closest('input,textarea,select,[data-no-drag]')) return;
        drag = { id: event.pointerId, start: axis === 'y' ? event.clientY : event.clientX, time: performance.now() };
        try { content.setPointerCapture?.(event.pointerId); } catch (error) { if (error?.name !== 'NotFoundError') throw error; }
        content.setAttribute('data-swiping', '');
    };
    const move = event => {
        if (!drag || drag.id !== event.pointerId) return;
        const position = axis === 'y' ? event.clientY : event.clientX;
        const delta = (position - drag.start) * sign;
        content.style.setProperty('--shadcn-drawer-drag', `${delta}px`);
    };
    const finish = (event, cancelled = false) => {
        if (!drag || drag.id !== event.pointerId) return;
        const position = axis === 'y' ? event.clientY : event.clientX;
        const delta = (position - drag.start) * sign;
        const span = axis === 'y' ? content.getBoundingClientRect().height : content.getBoundingClientRect().width;
        const velocity = Math.max(0, delta) / Math.max(1, performance.now() - drag.time);
        try { content.releasePointerCapture?.(event.pointerId); } catch (error) { if (error?.name !== 'NotFoundError') throw error; }
        content.removeAttribute('data-swiping'); content.style.removeProperty('--shadcn-drawer-drag'); drag = null;
        if (cancelled) { applyActiveSnap(); return; }
        if (snapPoints?.length && axis === 'y') {
            const resolved = resolvedSnapPoints();
            const activeIndex = resolved.findIndex(value => value.point === content.dataset.snapPoint);
            const target = Math.max(0, (activeIndex >= 0 ? resolved[activeIndex].pixels : span) - delta);
            const nearest = resolved.reduce((best, value) => Math.abs(value.pixels - target) < Math.abs(best.pixels - target) ? value : best);
            if (activeIndex >= 0 && nearest.index !== resolved[activeIndex].index) { dotnet.invokeMethodAsync('SetSnapPointIndexAsync', nearest.index); return; }
            if (delta > 0 && activeIndex > 0 && (delta > span * .3 || velocity > .7)) { dotnet.invokeMethodAsync('SetSnapPointIndexAsync', resolved[activeIndex - 1].index); return; }
            if (!disablePointerDismissal && delta > 0 && activeIndex === 0 && (delta > span * .3 || velocity > .7)) { dotnet.invokeMethodAsync('RequestCloseAsync'); return; }
            if (nearest) dotnet.invokeMethodAsync('SetSnapPointIndexAsync', nearest.index);
        }
        else if (!disablePointerDismissal && delta > 0 && (delta > span * .3 || velocity > .7)) dotnet.invokeMethodAsync('RequestCloseAsync');
    };
    const up = event => finish(event);
    const cancel = event => finish(event, true);
    const resize = () => applyActiveSnap();
    const outside = event => {
        if (modal || disablePointerDismissal || !isTopLayer(content) || content.contains(event.target)) return;
        const trigger = content.id ? document.querySelector(`[data-slot="drawer-trigger"][aria-controls="${CSS.escape(content.id)}"]`) : null;
        if (trigger?.contains(event.target)) return;
        event.preventDefault();
        dotnet.invokeMethodAsync('RequestCloseAsync');
    };
    content.addEventListener('pointerdown', down); content.addEventListener('pointermove', move); content.addEventListener('pointerup', up); content.addEventListener('pointercancel', cancel); addEventListener('resize', resize);
    if (!modal) document.addEventListener('pointerdown', outside);
    if (!trap) content.dataset.drawerFocusTrap = 'false';
    drawers.set(content, { down, move, up, cancel, resize, outside, modal, snapObserver });
}
export function detachDrawer(content) { const value = drawers.get(content); if (value) { content.removeEventListener('pointerdown', value.down); content.removeEventListener('pointermove', value.move); content.removeEventListener('pointerup', value.up); content.removeEventListener('pointercancel', value.cancel); removeEventListener('resize', value.resize); if (!value.modal) document.removeEventListener('pointerdown', value.outside); value.snapObserver.disconnect(); drawers.delete(content); } detachDialog(content); }

const positioned = new WeakMap();
function placePositioned(content, trigger, preferredSide, align, sideOffset, alignOffset, padding) {
    const anchor = trigger.getBoundingClientRect(), popup = { width: content.offsetWidth, height: content.offsetHeight };
    const space = { top: anchor.top - padding, bottom: innerHeight - anchor.bottom - padding, left: anchor.left - padding, right: innerWidth - anchor.right - padding };
    const opposite = { top: 'bottom', bottom: 'top', left: 'right', right: 'left' };
    const needed = preferredSide === 'top' || preferredSide === 'bottom' ? popup.height + sideOffset : popup.width + sideOffset;
    const side = space[preferredSide] < needed && space[opposite[preferredSide]] > space[preferredSide] ? opposite[preferredSide] : preferredSide;
    let left, top;
    const rtl = getComputedStyle(trigger).direction === 'rtl';
    if (side === 'top' || side === 'bottom') {
        top = side === 'top' ? anchor.top - popup.height - sideOffset : anchor.bottom + sideOffset;
        left = align === 'start' ? (rtl ? anchor.right - popup.width - alignOffset : anchor.left + alignOffset) : align === 'end' ? (rtl ? anchor.left - alignOffset : anchor.right - popup.width + alignOffset) : anchor.left + (anchor.width - popup.width) / 2 + alignOffset;
    } else {
        left = side === 'left' ? anchor.left - popup.width - sideOffset : anchor.right + sideOffset;
        top = align === 'start' ? anchor.top + alignOffset : align === 'end' ? anchor.bottom - popup.height + alignOffset : anchor.top + (anchor.height - popup.height) / 2 + alignOffset;
    }
    left = Math.min(Math.max(padding, left), Math.max(padding, innerWidth - padding - popup.width));
    top = Math.min(Math.max(padding, top), Math.max(padding, innerHeight - padding - popup.height));
    content.dataset.side = side; content.dataset.align = align; content.style.position = 'fixed'; content.style.left = `${left}px`; content.style.top = `${top}px`;
    content.style.setProperty('--shadcn-transform-origin', side === 'top' ? 'bottom' : side === 'bottom' ? 'top' : side === 'left' ? 'right' : 'left');
    content.dataset.positioned = 'true';
}
export function attachPositioned(content, triggerId, side, align, sideOffset, alignOffset, padding, dotnet, closeOnEscape, closeOnOutside, focusContent) {
    const trigger = document.getElementById(triggerId); if (!trigger) return;
    const previous = trigger;
    const sync = () => placePositioned(content, trigger, side, align, sideOffset, alignOffset, padding);
    acquireLayer(content);
    const keydown = event => { if (!event.__shadcnLayerHandled && isTopLayer(content) && event.key === 'Escape' && closeOnEscape) { event.__shadcnLayerHandled=true;event.preventDefault(); event.stopImmediatePropagation(); trigger.focus({ preventScroll: true }); dotnet.invokeMethodAsync('RequestCloseAsync'); } };
    const outside = event => { if (isTopLayer(content) && closeOnOutside && !content.contains(event.target) && !trigger.contains(event.target)) { trigger.focus({ preventScroll: true }); dotnet.invokeMethodAsync('RequestCloseAsync'); } };
    const observer = new ResizeObserver(sync); observer.observe(content); observer.observe(trigger);
    const anchorObserver = new MutationObserver(() => { if (!content.isConnected) detachPositioned(content); else if (!trigger.isConnected) dotnet?.invokeMethodAsync('RequestCloseAsync'); });
    anchorObserver.observe(document.body, { childList: true, subtree: true });
    addEventListener('resize', sync); addEventListener('scroll', sync, true); document.addEventListener('keydown', keydown); document.addEventListener('pointerdown', outside);
    queueMicrotask(() => { sync(); if (focusContent) (focusable(content)[0] || content).focus({ preventScroll: true }); });
    positioned.set(content, { trigger, previous, sync, keydown, outside, observer, anchorObserver });
}
export function detachPositioned(content) { const value = positioned.get(content); if (!value) return; releaseLayer(content); value.observer.disconnect(); value.anchorObserver.disconnect(); removeEventListener('resize', value.sync); removeEventListener('scroll', value.sync, true); document.removeEventListener('keydown', value.keydown); document.removeEventListener('pointerdown', value.outside); if (value.previous?.isConnected) value.previous.focus?.({ preventScroll: true }); positioned.delete(content); }
export function isPositionedAttached(content) { return positioned.has(content); }

const delayedTriggers = new WeakMap();
export function attachDelayedTrigger(trigger, dotnet, openDelay, closeDelay, contentId) {
    let timer = 0;
    const schedule = (open, delay) => { clearTimeout(timer); timer = setTimeout(() => dotnet.invokeMethodAsync('RequestOpenAsync', open), delay); };
    const enter = () => schedule(true, openDelay), leave = event => { const content = document.getElementById(contentId); if (content?.contains(event.relatedTarget)) return; schedule(false, closeDelay); };
    const focus = () => schedule(true, openDelay), blur = event => { const content = document.getElementById(contentId); if (content?.contains(event.relatedTarget)) return; schedule(false, closeDelay); };
    trigger.addEventListener('pointerenter', enter); trigger.addEventListener('pointerleave', leave); trigger.addEventListener('focus', focus); trigger.addEventListener('blur', blur);
    delayedTriggers.set(trigger, { enter, leave, focus, blur, clear: () => clearTimeout(timer) });
}
export function detachDelayedTrigger(trigger) { const value = delayedTriggers.get(trigger); if (!value) return; value.clear(); trigger.removeEventListener('pointerenter', value.enter); trigger.removeEventListener('pointerleave', value.leave); trigger.removeEventListener('focus', value.focus); trigger.removeEventListener('blur', value.blur); delayedTriggers.delete(trigger); }

const hoverContents = new WeakMap();
export function attachHoverContent(content, dotnet, closeDelay) {
    let timer = 0;
    const enter = () => clearTimeout(timer);
    const leave = event => { if (event.relatedTarget?.closest?.('[data-slot="hover-card-trigger"]')) return; clearTimeout(timer); timer = setTimeout(() => dotnet.invokeMethodAsync('RequestOpenAsync', false), closeDelay); };
    content.addEventListener('pointerenter', enter); content.addEventListener('pointerleave', leave); hoverContents.set(content, { enter, leave, clear: () => clearTimeout(timer) });
}
export function detachHoverContent(content) { const value = hoverContents.get(content); if (!value) return; value.clear(); content.removeEventListener('pointerenter', value.enter); content.removeEventListener('pointerleave', value.leave); hoverContents.delete(content); }

const menus = new WeakMap();
export function attachMenu(menu, triggerId, dotnet = null, loop = true) {
    acquireLayer(menu);
    const enabled = scope => [...scope.querySelectorAll('[role="menuitem"],[role="menuitemcheckbox"],[role="menuitemradio"]')].filter(item => item.closest('[role="menu"]') === scope && item.getAttribute('aria-disabled') !== 'true');
    let buffer = '', timer = 0;
    const focusAt = (scope, index) => { const items = enabled(scope); if (!items.length)return;const target=loop?(index+items.length)%items.length:Math.max(0,Math.min(index,items.length-1));items[target].focus({ preventScroll: true }); };
    const keydown = event => {
        const scope = document.activeElement?.closest('[role="menu"]') || menu;
        if (event.key === 'Escape' && scope !== menu) return;
        const items = enabled(scope), current = items.indexOf(document.activeElement), rtl = getComputedStyle(scope).direction === 'rtl';
        if (event.key === 'ArrowDown') { event.preventDefault(); focusAt(scope, current + 1); }
        else if (event.key === 'ArrowUp') { event.preventDefault(); focusAt(scope, current - 1); }
        else if (event.key === 'Home') { event.preventDefault(); focusAt(scope, 0); }
        else if (event.key === 'End') { event.preventDefault(); focusAt(scope, items.length - 1); }
        else if (event.key === 'Enter' || event.key === ' ') { if (document.activeElement?.matches('[role^="menuitem"]')) { event.preventDefault(); document.activeElement.click(); } }
        else if (!event.__shadcnLayerHandled && event.key === 'Escape' && isTopLayer(menu)) { event.__shadcnLayerHandled=true;event.preventDefault();event.stopImmediatePropagation(); document.getElementById(triggerId)?.focus({ preventScroll: true }); const closing = dotnet?.invokeMethodAsync('RequestCloseAsync'); closing?.then(() => requestAnimationFrame(() => document.getElementById(triggerId)?.focus({ preventScroll: true }))); }
        else if (event.key === 'Tab') { dotnet?.invokeMethodAsync('RequestCloseAsync'); }
        else if ((event.key === 'ArrowRight' && !rtl) || (event.key === 'ArrowLeft' && rtl)) { const sub = document.activeElement?.closest('[data-slot$="sub-trigger"]'); if (sub) { event.preventDefault(); if (sub.getAttribute('aria-expanded') !== 'true') sub.click(); requestAnimationFrame(() => requestAnimationFrame(() => sub.parentElement?.querySelector('[role="menu"] [role^="menuitem"]')?.focus({ preventScroll: true }))); } }
        else if ((event.key === 'ArrowLeft' && !rtl) || (event.key === 'ArrowRight' && rtl)) { if (scope !== menu) { event.preventDefault(); const sub = scope.parentElement?.querySelector(':scope > [data-slot$="sub-trigger"]'); if (sub?.getAttribute('aria-expanded') === 'true') sub.click(); sub?.focus({ preventScroll: true }); } }
        else if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) { clearTimeout(timer); const key = event.key.toLocaleLowerCase(); buffer += key; timer = setTimeout(() => buffer = '', 700); const search = [...buffer].every(value => value === key) ? key : buffer; const ordered = [...items.slice(Math.max(0, current + 1)), ...items.slice(0, Math.max(0, current + 1))]; const match = ordered.find(item => (item.dataset.textValue || item.textContent || '').trim().toLocaleLowerCase().startsWith(search)); if (match) { event.preventDefault(); match.focus({ preventScroll: true }); } }
    };
    const outside = event => { if (isTopLayer(menu) && !menu.contains(event.target) && !document.getElementById(triggerId)?.contains(event.target)) dotnet?.invokeMethodAsync('RequestCloseAsync'); };
    let hoverTimer = 0;
    const over = event => { const submenu = event.target.closest?.('[data-slot$="sub-content"]'); if (submenu && menu.contains(submenu)) { clearTimeout(hoverTimer); return; } const trigger = event.target.closest?.('[data-slot$="sub-trigger"]'); if (!trigger || !menu.contains(trigger) || trigger.getAttribute('aria-disabled') === 'true') return; clearTimeout(hoverTimer); if (trigger.getAttribute('aria-expanded') !== 'true') trigger.click(); };
    const out = event => { const trigger = event.target.closest?.('[data-slot$="sub-trigger"]'); if (!trigger || trigger.parentElement?.contains(event.relatedTarget)) return; hoverTimer = setTimeout(() => { if (trigger.getAttribute('aria-expanded') === 'true') trigger.click(); }, 300); };
    menu.addEventListener('keydown', keydown, true); menu.addEventListener('pointerover', over); menu.addEventListener('pointerout', out); document.addEventListener('pointerdown', outside); if (!menu.matches('[data-slot="menubar-content"]')) queueMicrotask(() => focusAt(menu, 0)); menus.set(menu, { keydown, outside, over, out, clear: () => { clearTimeout(timer); clearTimeout(hoverTimer); } });
}
export function detachMenu(menu) { const value = menus.get(menu); if (!value) return; releaseLayer(menu);value.clear(); menu.removeEventListener('keydown', value.keydown, true); menu.removeEventListener('pointerover', value.over); menu.removeEventListener('pointerout', value.out); document.removeEventListener('pointerdown', value.outside); menus.delete(menu); }
export function isMenuAttached(menu) { return menus.has(menu); }
export function focusFirstMenuItem(menu) { requestAnimationFrame(() => menu.querySelector('[role="menuitem"],[role="menuitemcheckbox"],[role="menuitemradio"]')?.focus({ preventScroll: true })); }
export function focusSubTrigger(menu) { menu.parentElement?.querySelector(':scope > [data-slot$="sub-trigger"]')?.focus({ preventScroll: true }); }

export function placeContextMenu(menu, padding = 8) {
    const desiredX = Number(menu.dataset.anchorX) || 0, desiredY = Number(menu.dataset.anchorY) || 0;
    const width = menu.getBoundingClientRect().width, height = menu.getBoundingClientRect().height;
    menu.style.left = `${Math.min(Math.max(padding, desiredX), Math.max(padding, innerWidth - padding - width))}px`;
    menu.style.top = `${Math.min(Math.max(padding, desiredY), Math.max(padding, innerHeight - padding - height))}px`;
}

const menubars = new WeakMap();
export function attachMenubar(menubar, loop = true) {
    const existing = menubars.get(menubar);
    if (existing) { existing.loop = loop; return; }
    const state = { loop, hoveredTrigger: null };
    const triggers = () => [...menubar.querySelectorAll(':scope > [data-slot="menubar-trigger"], :scope > * > [data-slot="menubar-trigger"]')].filter(item => !item.disabled);
    const indexAt = (index, length) => state.loop ? (index + length) % length : Math.max(0, Math.min(index, length - 1));
    const focusAt = index => { const items = triggers(); if (!items.length) return; const target = indexAt(index, items.length); items.forEach((item, itemIndex) => item.tabIndex = itemIndex === target ? 0 : -1); items[target].focus({ preventScroll: true }); };
    let buffer = '', timer = 0;
    const switchOpen = target => { const open = menubar.querySelector('[data-slot="menubar-trigger"][aria-expanded="true"]'); if (open && open !== target) target.click(); };
    const keydown = event => {
        const items = triggers(), activeTrigger = document.activeElement?.closest?.('[data-slot="menubar-trigger"]'), current = activeTrigger ? items.indexOf(activeTrigger) : items.findIndex(item => item.getAttribute('aria-expanded') === 'true'), rtl = getComputedStyle(menubar).direction === 'rtl';
        if (!items.length) return;
        if ((event.key === 'ArrowRight' && !rtl) || (event.key === 'ArrowLeft' && rtl)) { event.preventDefault(); const target = items[indexAt(current < 0 ? 0 : current + 1, items.length)]; switchOpen(target); requestAnimationFrame(() => requestAnimationFrame(() => target.focus({ preventScroll: true }))); }
        else if ((event.key === 'ArrowLeft' && !rtl) || (event.key === 'ArrowRight' && rtl)) { event.preventDefault(); const target = items[indexAt(current < 0 ? items.length - 1 : current - 1, items.length)]; switchOpen(target); requestAnimationFrame(() => requestAnimationFrame(() => target.focus({ preventScroll: true }))); }
        else if (event.key === 'Home') { event.preventDefault(); focusAt(0); }
        else if (event.key === 'End') { event.preventDefault(); focusAt(items.length - 1); }
        else if ((event.key === 'ArrowDown' || event.key === 'Enter' || event.key === ' ') && document.activeElement?.matches('[data-slot="menubar-trigger"]')) { event.preventDefault(); document.activeElement.click(); }
        else if (event.key.length === 1 && !event.ctrlKey && !event.metaKey && !event.altKey) { clearTimeout(timer); const key = event.key.toLocaleLowerCase(); buffer += key; timer = setTimeout(() => buffer = '', 700); const search = [...buffer].every(value => value === key) ? key : buffer; const ordered = [...items.slice(Math.max(0,current+1)),...items.slice(0,Math.max(0,current+1))]; const match=ordered.find(item=>(item.dataset.textValue||item.textContent||'').trim().toLocaleLowerCase().startsWith(search)); if(match){event.preventDefault();focusAt(items.indexOf(match));} }
    };
    const over = event => {
        if (event.pointerType === 'touch') return;
        const target = event.target.closest?.('[data-slot="menubar-trigger"]');
        if (!target || !menubar.contains(target) || target.disabled || target.contains(event.relatedTarget)) return;
        const open = menubar.querySelector('[data-slot="menubar-trigger"][aria-expanded="true"]');
        if (!open || open === target) { state.hoveredTrigger = target; return; }
        if (state.hoveredTrigger === target) return;
        state.hoveredTrigger = target;
        target.click();
    };
    const leave = event => { if (!menubar.contains(event.relatedTarget)) state.hoveredTrigger = null; };
    menubar.addEventListener('keydown', keydown); menubar.addEventListener('pointerover', over); menubar.addEventListener('pointerleave', leave); Object.assign(state, { keydown, over, leave, clear:()=>clearTimeout(timer) }); menubars.set(menubar, state);
}
export function detachMenubar(menubar) { const value = menubars.get(menubar); if (!value) return; value.clear(); menubar.removeEventListener('keydown', value.keydown); menubar.removeEventListener('pointerover', value.over); menubar.removeEventListener('pointerleave', value.leave); menubars.delete(menubar); }

const commands = new WeakMap();
export function attachCommand(command, loop = true) {
    const visibleItems = () => [...command.querySelectorAll('[data-slot="command-item"]:not([hidden])')];
    const items = () => visibleItems().filter(item => item.getAttribute('aria-disabled') !== 'true');
    const allItems = () => [...command.querySelectorAll('[data-slot="command-item"]')];
    const input = command.querySelector('[data-slot="command-input"]');
    let selectedValue = null, composing = false;
    const selectAt = index => { const values = items(); if (!values.length) { selectedValue=null; input?.removeAttribute('aria-activedescendant'); return; } const selected = loop ? (index + values.length) % values.length : Math.max(0, Math.min(index, values.length - 1)); values.forEach((item, itemIndex) => { const active = itemIndex === selected; item.dataset.selected = active ? 'true' : 'false'; item.setAttribute('aria-selected', active ? 'true' : 'false'); }); selectedValue=values[selected].dataset.value; input?.setAttribute('aria-activedescendant',values[selected].id); values[selected].scrollIntoView({ block: 'nearest' }); };
    const selectedIndex = () => items().findIndex(item => item.dataset.value === selectedValue);
    const keydown = event => {
        if (event.isComposing || composing) return;
        const selected=selectedIndex();
        if (event.key === 'ArrowDown') { event.preventDefault(); selectAt(selected + 1); }
        else if (event.key === 'ArrowUp') { event.preventDefault(); selectAt(selected - 1); }
        else if (event.key === 'Home') { event.preventDefault(); selectAt(0); }
        else if (event.key === 'End') { event.preventDefault(); selectAt(items().length - 1); }
        else if (event.key === 'PageDown') { event.preventDefault(); selectAt(selected + 10); }
        else if (event.key === 'PageUp') { event.preventDefault(); selectAt(selected - 10); }
        else if (event.key === 'Enter' && selected >= 0) { event.preventDefault(); items()[selected]?.click(); }
    };
    const filter = () => {
        const query = (input?.value || '').normalize('NFKC').toLocaleLowerCase();
        const shouldFilter=command.dataset.shouldFilter !== 'false';
        if(shouldFilter) allItems().forEach(item => item.hidden = query.length > 0 && !(item.dataset.searchText || '').normalize('NFKC').toLocaleLowerCase().includes(query));
        const empty = command.querySelector('[data-slot="command-empty"]'); if (empty) empty.hidden = visibleItems().length > 0;
        command.querySelectorAll('[data-slot="command-group"]').forEach(group=>group.hidden=!group.querySelector('[data-slot="command-item"]:not([hidden])'));
        const index=selectedIndex(); if(index>=0)selectAt(index);else if(items().length)selectAt(0);else{selectedValue=null;input?.removeAttribute('aria-activedescendant');}
    };
    const pointermove = event => {
        const target = event.target.closest?.('[data-slot="command-item"]:not([hidden])');
        if (!target || !command.contains(target) || target.getAttribute('aria-disabled') === 'true') return;
        const index = items().indexOf(target);
        if (index >= 0 && index !== selectedIndex()) selectAt(index);
    };
    const compositionstart=()=>composing=true,compositionend=()=>{composing=false;filter();};
    command.addEventListener('keydown', keydown); command.addEventListener('pointermove', pointermove); input?.addEventListener('input', filter);input?.addEventListener('compositionstart',compositionstart);input?.addEventListener('compositionend',compositionend); filter(); commands.set(command, { keydown, pointermove, input, filter,compositionstart,compositionend });
}
export function refreshCommand(command){commands.get(command)?.filter();}
export function detachCommand(command) { const value = commands.get(command); if (!value) return; command.removeEventListener('keydown', value.keydown); command.removeEventListener('pointermove', value.pointermove); value.input?.removeEventListener('input', value.filter);value.input?.removeEventListener('compositionstart',value.compositionstart);value.input?.removeEventListener('compositionend',value.compositionend); commands.delete(command); }
