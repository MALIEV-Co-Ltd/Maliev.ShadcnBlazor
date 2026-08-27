function focusGroupControl(addon, eventTarget) {
    const group = addon?.closest?.('[data-slot="input-group"]');
    const target = group?.querySelector?.('input:not([disabled]), textarea:not([disabled]), select:not([disabled])');
    const interactiveTarget = eventTarget?.closest?.('button, a, input, textarea, select, [role="button"]');
    if (target && !interactiveTarget) target.focus();
}

const groupAddonHandlers = new WeakMap();
export function wireGroupAddon(addon) {
    unwireGroupAddon(addon);
    const handler = event => focusGroupControl(addon, event.target);
    addon.addEventListener('click', handler);
    groupAddonHandlers.set(addon, handler);
}
export function unwireGroupAddon(addon) {
    const handler = groupAddonHandlers.get(addon);
    if (handler) addon.removeEventListener('click', handler);
    groupAddonHandlers.delete(addon);
}

export function restoreSelectValue(select, value) {
    if (select) select.value = value ?? '';
}

const otpObservers = new WeakMap();

export function observeOtpSelection(input, dotnet, maxLength) {
    disconnectOtpSelection(input);
    const nextGraphemeOffset = offset => {
        const tail = input.value.slice(offset);
        if (!tail) return offset;
        if (typeof Intl.Segmenter === 'function') {
            const segment = new Intl.Segmenter(undefined, { granularity: 'grapheme' }).segment(tail)[Symbol.iterator]().next().value;
            return offset + segment.segment.length;
        }
        return offset + Array.from(tail)[0].length;
    };
    const update = () => {
        const offset = input.selectionStart ?? input.value.length;
        const beforeCaret = input.value.slice(0, offset);
        const count = typeof Intl.Segmenter === 'function'
            ? [...new Intl.Segmenter(undefined, { granularity: 'grapheme' }).segment(beforeCaret)].length
            : Array.from(beforeCaret).length;
        dotnet.invokeMethodAsync('UpdateOtpSelection', Math.min(count, Math.max(0, maxLength - 1)), document.activeElement === input);
    };
    const onInput = event => {
        const start = input.selectionStart;
        const shouldSelectNext = event.inputType?.startsWith('insert') && start !== null && start === input.selectionEnd && start < input.value.length;
        const end = shouldSelectNext ? nextGraphemeOffset(start) : start;
        if (shouldSelectNext) input.setSelectionRange(start, end);
        update();
        if (shouldSelectNext) requestAnimationFrame(() => {
            if (document.activeElement !== input) return;
            input.setSelectionRange(start, Math.min(end, input.value.length));
            update();
        });
    };
    const onPointerDown = event => {
        if (event.button !== 0) return;
        const root = input.closest('[data-slot="input-otp-root"]');
        const slots = [...(root?.querySelectorAll('[data-slot="input-otp-slot"]') ?? [])];
        if (!slots.length) return;
        const point = event.clientX;
        let index = slots.findIndex(slot => {
            const bounds = slot.getBoundingClientRect();
            return point >= bounds.left && point <= bounds.right;
        });
        if (index < 0) index = point < slots[0].getBoundingClientRect().left ? 0 : slots.length - 1;
        event.preventDefault();
        input.focus({ preventScroll: true });
        const end = index < input.value.length ? index + 1 : index;
        input.setSelectionRange(index, end);
        update();
    };
    input.addEventListener('input', onInput);
    for (const eventName of ['keyup', 'click', 'select', 'focus', 'blur']) input.addEventListener(eventName, update);
    input.addEventListener('pointerdown', onPointerDown);
    otpObservers.set(input, { update, onInput, onPointerDown });
    queueMicrotask(update);
}

export function disconnectOtpSelection(input) {
    const observer = otpObservers.get(input);
    if (!observer) return;
    input.removeEventListener('input', observer.onInput);
    for (const eventName of ['keyup', 'click', 'select', 'focus', 'blur']) input.removeEventListener(eventName, observer.update);
    input.removeEventListener('pointerdown', observer.onPointerDown);
    otpObservers.delete(input);
}

const popupObservers = new WeakMap();

export function observePopupDismissal(root, dotnet, kind) {
    disconnectPopupDismissal(root);
    const onPointerDown = event => { if (!root.contains(event.target)) dotnet.invokeMethodAsync('DismissPopup'); };
    const onKeyDown = event => {
        const editable = event.target?.matches?.('input, textarea, [contenteditable="true"]');
        const keys = kind === 'select'
            ? ['ArrowDown', 'ArrowUp', 'Home', 'End', 'Enter', ' ', 'Escape']
            : kind === 'combobox'
                ? ['ArrowDown', 'ArrowUp', 'Enter', 'Escape']
                : editable ? ['Escape'] : ['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Enter', ' ', 'Escape', 'PageUp', 'PageDown'];
        if (keys.includes(event.key)) event.preventDefault();
    };
    document.addEventListener('pointerdown', onPointerDown, true);
    root.addEventListener('keydown', onKeyDown, true);
    popupObservers.set(root, { onPointerDown, onKeyDown });
}

export function disconnectPopupDismissal(root) {
    const observer = popupObservers.get(root);
    if (!observer) return;
    document.removeEventListener('pointerdown', observer.onPointerDown, true);
    root.removeEventListener('keydown', observer.onKeyDown, true);
    popupObservers.delete(root);
}

export function focusElement(element) {
    element?.focus?.({ preventScroll: true });
}

export function focusCalendarInPopup(root) {
    queueMicrotask(() => root?.querySelector?.('[data-slot="date-picker-content"] [data-slot="calendar-day"][tabindex="0"]:not(:disabled)')?.focus?.({ preventScroll: true }));
}

export function focusCalendarDay(root, isoDate) {
    root?.querySelector?.(`[data-slot="calendar-day"][data-day="${CSS.escape(isoDate)}"]`)?.focus();
}
