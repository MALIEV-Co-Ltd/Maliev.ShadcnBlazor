export function observeMasonry(root) {
    const layout = root.querySelector(":scope > [data-slot='bento-grid-layout']");
    if (!layout) return { dispose() {} };

    let frame = 0;
    const measure = () => {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            if (!root.isConnected) return;
            const styles = getComputedStyle(layout);
            const row = Number.parseFloat(styles.gridAutoRows) || 8;
            const gap = Number.parseFloat(styles.rowGap) || 0;
            const items = Array.from(layout.children).filter(item => item.matches("[data-slot='bento-item']"));

            for (const item of items) {
                const content = item.firstElementChild;
                const height = Math.max(content?.scrollHeight ?? 0, content?.getBoundingClientRect().height ?? 0, item.scrollHeight);
                const span = Math.max(1, Math.ceil((height + gap) / (row + gap)));
                if (item.style.getPropertyValue("--shadcn-bento-masonry-span") !== String(span))
                    item.style.setProperty("--shadcn-bento-masonry-span", String(span));
            }
            root.dataset.masonryReady = "true";
        });
    };

    const resize = new ResizeObserver(measure);
    resize.observe(root);
    for (const item of layout.children) resize.observe(item);
    const mutations = new MutationObserver(() => {
        resize.disconnect();
        resize.observe(root);
        for (const item of layout.children) resize.observe(item);
        measure();
    });
    mutations.observe(layout, { childList: true });
    measure();

    return {
        dispose() {
            cancelAnimationFrame(frame);
            resize.disconnect();
            mutations.disconnect();
            root.removeAttribute("data-masonry-ready");
            for (const item of layout.children) item.style.removeProperty("--shadcn-bento-masonry-span");
        }
    };
}
