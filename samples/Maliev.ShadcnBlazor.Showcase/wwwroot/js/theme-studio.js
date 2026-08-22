export function focusElementById(id) {
    const element = document.getElementById(id);
    if (element instanceof HTMLElement) {
        element.focus({ preventScroll: true });
    }
}

export function isDesktopWorkbench() {
    return window.matchMedia("(min-width: 64.0625rem)").matches;
}
