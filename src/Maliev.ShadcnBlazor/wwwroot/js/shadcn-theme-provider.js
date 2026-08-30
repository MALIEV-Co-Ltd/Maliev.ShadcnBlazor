export function observeSystemDarkMode(dotNetReference) {
    const query = window.matchMedia("(prefers-color-scheme: dark)");
    const changed = event => dotNetReference.invokeMethodAsync("NotifySystemDarkModeChanged", event.matches);
    query.addEventListener("change", changed);

    return {
        getCurrent: () => query.matches,
        dispose: () => query.removeEventListener("change", changed)
    };
}
