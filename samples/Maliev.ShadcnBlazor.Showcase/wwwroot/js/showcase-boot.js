(() => {
    const boot = document.querySelector('.showcase-boot');
    if (!boot) return;

    const status = boot.querySelector('[data-boot-status]');
    const detail = boot.querySelector('[data-boot-detail]');
    const retry = boot.querySelector('[data-boot-retry]');
    let slowTimer;

    const setState = (state, message, description, canRetry) => {
        boot.dataset.state = state;
        boot.setAttribute('aria-busy', state === 'loading' ? 'true' : 'false');
        boot.setAttribute('role', state === 'error' ? 'alert' : 'status');
        boot.setAttribute('aria-live', state === 'error' ? 'assertive' : 'polite');
        status.textContent = message;
        detail.textContent = description;
        detail.hidden = !description;
        retry.hidden = !canRetry;
    };

    retry.addEventListener('click', () => window.location.reload());
    slowTimer = window.setTimeout(() => {
        setState(
            'slow',
            'Still loading showcase',
            'The component library is taking longer than expected to start.',
            true);
    }, 8_000);

    const start = async () => {
        try {
            if (!window.Blazor?.start) throw new Error('The Blazor runtime did not load.');
            await window.Blazor.start();
            window.clearTimeout(slowTimer);
        }
        catch (error) {
            window.clearTimeout(slowTimer);
            setState(
                'error',
                'Showcase could not start',
                'Check your connection, then retry this page.',
                true);
            console.error('Showcase startup failed.', error);
        }
    };

    start();
})();
