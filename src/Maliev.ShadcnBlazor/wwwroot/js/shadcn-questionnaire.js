export function attach(root, dotnet) {
    let disposed = false;

    const onKeyDown = (event) => {
        if (disposed || event.isComposing || event.altKey || event.ctrlKey || event.metaKey) {
            return;
        }

        const target = event.target;
        if (target instanceof Element && target.matches("input:not([type='radio']):not([type='checkbox']), textarea, [contenteditable='true']")) {
            return;
        }

        const mode = root.dataset.shortcuts;
        let index = -1;
        if (mode === "numbers" && /^[1-9]$/.test(event.key)) {
            index = Number(event.key) - 1;
        } else if (mode === "letters" && /^[a-z]$/i.test(event.key)) {
            index = event.key.toLowerCase().charCodeAt(0) - 97;
        }

        if (index >= 0) {
            event.preventDefault();
            void dotnet.invokeMethodAsync("OnShortcutAsync", index);
        }
    };

    root.addEventListener("keydown", onKeyDown);

    return {
        focusItem(name) {
            const item = Array.from(root.querySelectorAll("[data-slot='questionnaire-item']"))
                .find((candidate) => candidate.getAttribute("name") === name);
            const target = item?.querySelector("input:not(:disabled), button:not(:disabled), [tabindex='0']") ?? item;
            target?.focus({ preventScroll: true });
        },
        dispose() {
            disposed = true;
            root.removeEventListener("keydown", onKeyDown);
        },
    };
}
