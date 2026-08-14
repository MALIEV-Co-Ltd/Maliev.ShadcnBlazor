export function observe(element, receiver) {
    let frame = 0;
    const observer = new ResizeObserver(() => {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            if (!element.isConnected) return;
            const rect = element.getBoundingClientRect();
            element.style.setProperty("--shadcn-chart-width", `${rect.width}px`);
            element.dataset.chartMeasured = rect.width > 0 && rect.height > 0 ? "true" : "false";
            if (rect.width > 0 && rect.height > 0) receiver.invokeMethodAsync("OnChartResize", rect.width, rect.height);
        });
    });
    observer.observe(element);
    return { dispose() { cancelAnimationFrame(frame); observer.disconnect(); } };
}
