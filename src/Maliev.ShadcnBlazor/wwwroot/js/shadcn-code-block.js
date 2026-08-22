export async function copyText(text) {
  if (navigator.clipboard?.writeText) {
    try {
      await navigator.clipboard.writeText(text);
      return;
    } catch {
      // Restricted and embedded contexts can reject Clipboard API writes.
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
