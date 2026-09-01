export function observe(element) {
    let intersecting = false;

    const update = () => {
        const active = intersecting && document.visibilityState === "visible";
        element.setAttribute("data-loop-active", active ? "true" : "false");
    };

    const intersectionObserver = new IntersectionObserver(entries => {
        intersecting = entries.some(entry => entry.isIntersecting && entry.intersectionRatio >= 0.2);
        update();
    }, { threshold: [0, 0.2, 0.6] });

    const visibilityHandler = () => update();
    document.addEventListener("visibilitychange", visibilityHandler);
    intersectionObserver.observe(element);
    update();

    return {
        dispose() {
            intersectionObserver.disconnect();
            document.removeEventListener("visibilitychange", visibilityHandler);
            element.setAttribute("data-loop-active", "false");
        }
    };
}
