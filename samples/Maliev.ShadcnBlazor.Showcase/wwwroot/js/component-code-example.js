export async function copyText(source) {
    if (!navigator.clipboard?.writeText) {
        throw new Error("Clipboard writes are unavailable in this browser.");
    }

    await navigator.clipboard.writeText(source);
}
