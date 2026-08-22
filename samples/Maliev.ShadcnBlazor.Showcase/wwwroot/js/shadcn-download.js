export function downloadBytes(fileName, mimeType, bytes) {
    let objectUrl;

    try {
        const blob = new Blob([bytes], { type: mimeType });
        objectUrl = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = objectUrl;
        anchor.download = fileName;
        anchor.hidden = true;
        document.body.appendChild(anchor);

        try {
            anchor.click();
        } finally {
            anchor.remove();
        }
    } finally {
        if (objectUrl) {
            URL.revokeObjectURL(objectUrl);
        }
    }
}

export async function copyText(text) {
    if (navigator.clipboard?.writeText) {
        try {
            await navigator.clipboard.writeText(text);
            return;
        } catch {
            // Continue to the selection-based fallback used by restricted contexts.
        }
    }

    const activeElement = document.activeElement;
    const input = document.createElement("textarea");
    input.value = text;
    input.readOnly = true;
    input.setAttribute("aria-hidden", "true");
    input.style.position = "fixed";
    input.style.inset = "0 auto auto -9999px";
    document.body.appendChild(input);
    try {
        input.select();
        if (!document.execCommand("copy")) {
            throw new Error("Clipboard access is unavailable in this browser.");
        }
    } finally {
        input.remove();
        if (activeElement instanceof HTMLElement) {
            activeElement.focus({ preventScroll: true });
        }
    }
}
