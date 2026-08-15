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
    if (!navigator.clipboard?.writeText) {
        throw new Error("Clipboard access is unavailable in this browser.");
    }
    await navigator.clipboard.writeText(text);
}
