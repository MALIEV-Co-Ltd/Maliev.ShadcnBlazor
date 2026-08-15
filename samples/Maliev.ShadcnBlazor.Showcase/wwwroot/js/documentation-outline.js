let scrollHandler;
let frame;

function setActive(links, id) {
    for (const link of links) {
        const active = link.dataset.sectionLink === id;
        link.toggleAttribute("aria-current", active);
        link.dataset.active = active ? "true" : "false";
    }
}

export function observe(contentSelector, linkSelector) {
    dispose();

    const content = document.querySelector(contentSelector);
    const links = [...document.querySelectorAll(linkSelector)];
    if (!content || links.length === 0)
        return;

    const sections = links
        .map(link => document.getElementById(link.dataset.sectionLink))
        .filter(Boolean);

    const update = () => {
        frame = undefined;
        const headerOffset = 112;
        const candidates = sections
            .map(section => ({ section, top: section.getBoundingClientRect().top }))
            .filter(entry => entry.top <= headerOffset + 12);
        const current = (candidates.length > 0 ? candidates[candidates.length - 1].section : sections[0]);
        if (current)
            setActive(links, current.id);
    };

    scrollHandler = () => {
        if (frame === undefined)
            frame = requestAnimationFrame(update);
    };

    window.addEventListener("scroll", scrollHandler, { passive: true });
    window.addEventListener("resize", scrollHandler, { passive: true });
    update();
}

export function dispose() {
    if (scrollHandler) {
        window.removeEventListener("scroll", scrollHandler);
        window.removeEventListener("resize", scrollHandler);
        scrollHandler = undefined;
    }

    if (frame !== undefined) {
        cancelAnimationFrame(frame);
        frame = undefined;
    }
}
