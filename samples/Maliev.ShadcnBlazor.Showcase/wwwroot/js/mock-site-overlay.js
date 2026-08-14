window.mockSiteOverlay = (() => {
    const selectors = 'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
    const handlers = new WeakMap();
    const backgrounds = new WeakMap();

    function focusable(container) {
        return [...container.querySelectorAll(selectors)].filter(element => !element.hidden && element.getAttribute('aria-hidden') !== 'true');
    }

    function open(container) {
        close(container);
        const backdrop = container.parentElement;
        const root = container.closest('[data-mock-site]');
        const background = [];
        let branch = backdrop;
        while (branch && branch !== root) {
            for (const element of branch.parentElement.children) {
                if (element === branch) continue;
                background.push({
                    element,
                    inert: element.inert,
                    hadAriaHidden: element.hasAttribute('aria-hidden'),
                    ariaHidden: element.getAttribute('aria-hidden')
                });
                element.inert = true;
                element.setAttribute('aria-hidden', 'true');
            }
            branch = branch.parentElement;
        }
        backgrounds.set(container, background);
        const handler = event => {
            if (event.key !== 'Tab') return;
            const items = focusable(container);
            if (items.length === 0) return;
            const first = items[0];
            const last = items[items.length - 1];
            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault();
                last.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault();
                first.focus();
            }
        };
        handlers.set(container, handler);
        container.addEventListener('keydown', handler);
        focusable(container)[0]?.focus();
    }

    function close(container) {
        const handler = handlers.get(container);
        if (handler) container.removeEventListener('keydown', handler);
        handlers.delete(container);
        const background = backgrounds.get(container) || [];
        background.forEach(entry => {
            entry.element.inert = entry.inert;
            if (entry.hadAriaHidden) entry.element.setAttribute('aria-hidden', entry.ariaHidden);
            else entry.element.removeAttribute('aria-hidden');
        });
        backgrounds.delete(container);
    }

    function focusById(id) {
        document.getElementById(id)?.focus();
    }

    return { open, close, focusById };
})();
