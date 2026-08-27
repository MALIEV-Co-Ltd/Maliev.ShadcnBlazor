export function observeMasonry(root) {
    const layout = root.querySelector(":scope > [data-slot='bento-grid-layout']");
    if (!layout) return { dispose() {} };

    let frame = 0;
    const items = () => Array.from(layout.children).filter(item => item.matches("[data-slot='bento-item']"));
    const contentFor = item => item.firstElementChild ?? item;
    const measure = () => {
        cancelAnimationFrame(frame);
        frame = requestAnimationFrame(() => {
            if (!root.isConnected) return;
            const styles = getComputedStyle(layout);
            const row = Number.parseFloat(styles.gridAutoRows) || 1;
            const gap = Number.parseFloat(styles.rowGap) || 0;

            for (const item of items()) {
                const content = contentFor(item);
                const height = Math.max(content.scrollHeight, content.getBoundingClientRect().height);
                const span = Math.max(1, Math.ceil((height + gap) / (row + gap)));
                if (item.style.getPropertyValue("--shadcn-bento-masonry-span") !== String(span))
                    item.style.setProperty("--shadcn-bento-masonry-span", String(span));
            }
            root.dataset.masonryReady = "true";
        });
    };

    const resize = new ResizeObserver(measure);
    const observeSizes = () => {
        resize.disconnect();
        resize.observe(root);
        for (const item of items()) resize.observe(contentFor(item));
    };
    observeSizes();
    const mutations = new MutationObserver(() => {
        observeSizes();
        measure();
    });
    mutations.observe(layout, { childList: true, subtree: true });
    measure();

    return {
        dispose() {
            cancelAnimationFrame(frame);
            resize.disconnect();
            mutations.disconnect();
            root.removeAttribute("data-masonry-ready");
            for (const item of items()) item.style.removeProperty("--shadcn-bento-masonry-span");
        }
    };
}
