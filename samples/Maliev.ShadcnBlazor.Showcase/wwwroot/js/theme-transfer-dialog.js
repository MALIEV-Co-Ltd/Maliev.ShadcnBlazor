const registrations = new Map();

export function connect(id, dotNetReference) {
    const dialog = document.getElementById(id);
    if (!(dialog instanceof HTMLDialogElement)) {
        throw new Error(`Dialog '${id}' was not found.`);
    }

    disconnect(id);
    const onClose = () => dotNetReference.invokeMethodAsync("OnNativeClosed");
    dialog.addEventListener("close", onClose);
    registrations.set(id, { dialog, onClose });
}

export function showModal(id) {
    const registration = registrations.get(id);
    const dialog = registration?.dialog ?? document.getElementById(id);
    if (!(dialog instanceof HTMLDialogElement)) {
        throw new Error(`Dialog '${id}' was not found.`);
    }

    if (!dialog.open) {
        dialog.showModal();
    }
}

export function close(id, notify = false) {
    const registration = registrations.get(id);
    const dialog = registration?.dialog ?? document.getElementById(id);
    if (dialog instanceof HTMLDialogElement && dialog.open) {
        if (!notify && registration) {
            dialog.removeEventListener("close", registration.onClose);
        }
        dialog.close();
        if (!notify && registration) {
            dialog.addEventListener("close", registration.onClose);
        }
    }
}

export function disconnect(id) {
    const registration = registrations.get(id);
    if (!registration) return;
    registration.dialog.removeEventListener("close", registration.onClose);
    registrations.delete(id);
}
