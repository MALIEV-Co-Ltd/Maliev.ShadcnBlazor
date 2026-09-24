export function observe(element, receiver) {
    let frame = 0;
    const surface = element.querySelector('[data-slot="chart-surface"]');
    let activeTooltip = null;
    const positionTooltip = () => {
        if (!element.isConnected || !surface) return;
        const tooltip = element.querySelector('[data-slot="chart-tooltip-content"]');
        if (activeTooltip && activeTooltip !== tooltip && activeTooltip.matches(':popover-open')) activeTooltip.hidePopover();
        activeTooltip = tooltip;
        if (!tooltip || typeof tooltip.showPopover !== 'function') return;

        tooltip.popover = 'manual';
        if (!tooltip.matches(':popover-open')) tooltip.showPopover();
        const surfaceRect = surface.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();
        const tooltipX = Number.parseFloat(tooltip.style.getPropertyValue('--shadcn-chart-tooltip-x'));
        const tooltipY = Number.parseFloat(tooltip.style.getPropertyValue('--shadcn-chart-tooltip-y'));
        const xPercent = Number.isFinite(tooltipX) ? tooltipX : 50;
        const yInChart = Number.isFinite(tooltipY) ? tooltipY : 48;
        const anchorX = surfaceRect.left + surfaceRect.width * xPercent / 100;
        const anchorY = surfaceRect.top + surfaceRect.height * yInChart / surface.viewBox.baseVal.height;
        const gap = 8;
        const left = Math.max(gap, Math.min(anchorX - tooltipRect.width / 2, innerWidth - tooltipRect.width - gap));
        const above = anchorY - tooltipRect.height - gap;
        const below = anchorY + gap;
        const top = Math.max(gap, Math.min(above >= gap ? above : below, innerHeight - tooltipRect.height - gap));
        tooltip.style.left = `${left}px`;
        tooltip.style.top = `${top}px`;
        tooltip.dataset.positioned = 'true';
    };
    const schedulePosition = () => {
        if (!activeTooltip && !element.querySelector('[data-slot="chart-tooltip-content"]')) return;
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(positionTooltip);
    };
    const mutations = new MutationObserver(schedulePosition);
    mutations.observe(element, { childList: true, subtree: true, attributes: true, attributeFilter: ['data-active-point'] });
    addEventListener('scroll', schedulePosition, true);
    addEventListener('resize', schedulePosition);
    const observer = new ResizeObserver(() => {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            if (!element.isConnected) return;
            const rect = (surface ?? element).getBoundingClientRect();
            element.style.setProperty("--shadcn-chart-width", `${rect.width}px`);
            element.dataset.chartMeasured = rect.width > 0 && rect.height > 0 ? "true" : "false";
            if (rect.width > 0 && rect.height > 0) receiver.invokeMethodAsync("OnChartResize", rect.width, rect.height);
            schedulePosition();
        });
    });
    observer.observe(surface ?? element);
    schedulePosition();
    return { dispose() { cancelAnimationFrame(frame); observer.disconnect(); mutations.disconnect(); removeEventListener('scroll', schedulePosition, true); removeEventListener('resize', schedulePosition); if (activeTooltip?.matches(':popover-open')) activeTooltip.hidePopover(); } };
}
