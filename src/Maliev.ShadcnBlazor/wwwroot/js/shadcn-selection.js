export function setIndeterminate(element, value) {
    if (element) {
        element.indeterminate = Boolean(value)
    }
}

const listeners = new WeakMap()

export function attachRovingGroup(root, kind, orientation, readOnly) {
    if (!root) return

    detach(root)
    const selector = kind === "radio"
        ? "[data-slot='radio-group-item']"
        : "[data-slot='toggle-group-item']"

    const getItems = () => Array.from(root.querySelectorAll(selector))
        .filter((item) => !item.disabled && item.getAttribute("aria-disabled") !== "true")

    const normalizeTabStops = () => {
        const items = getItems()
        if (items.length === 0) return
        const current = items.find((item) => item.tabIndex === 0 && (kind !== "radio" || item.checked))
            ?? items.find((item) => kind === "radio" && item.checked)
            ?? items.find((item) => item.tabIndex === 0)
            ?? items[0]
        for (const item of items) item.tabIndex = item === current ? 0 : -1
    }

    const listener = (event) => {
        const items = getItems()
        const currentIndex = items.indexOf(event.target)
        if (currentIndex < 0 || items.length === 0) return

        const rtl = getComputedStyle(root).direction === "rtl"
        let targetIndex = -1
        if (event.key === "Home") targetIndex = 0
        else if (event.key === "End") targetIndex = items.length - 1
        else if (orientation === "horizontal" && event.key === "ArrowRight") targetIndex = currentIndex + (rtl ? -1 : 1)
        else if (orientation === "horizontal" && event.key === "ArrowLeft") targetIndex = currentIndex + (rtl ? 1 : -1)
        else if (orientation === "vertical" && event.key === "ArrowDown") targetIndex = currentIndex + 1
        else if (orientation === "vertical" && event.key === "ArrowUp") targetIndex = currentIndex - 1
        else return

        event.preventDefault()
        targetIndex = (targetIndex + items.length) % items.length
        const target = items[targetIndex]
        for (const item of items) item.tabIndex = item === target ? 0 : -1
        target.focus()
        if (kind === "radio" && !readOnly) target.click()
    }

    root.addEventListener("keydown", listener)
    listeners.set(root, listener)
    normalizeTabStops()
}

export function attachSlider(root, readOnly) {
    if (!root) return

    const existing = listeners.get(root)
    if (existing && typeof existing === "object" && existing.kind === "slider") {
        existing.readOnly = readOnly
        existing.snapshotValues()
        return
    }

    detach(root)
    const inputs = () => Array.from(root.querySelectorAll("input[type='range']"))
    let readOnlyValues = inputs().map((input) => input.value)
    const snapshotValues = () => {
        readOnlyValues = inputs().map((input) => input.value)
    }
    const restoreReadOnlyValues = () => {
        inputs().forEach((input, index) => {
            if (readOnlyValues[index] !== undefined) input.value = readOnlyValues[index]
        })
    }

    const state = {
        kind: "slider",
        get readOnly() { return readOnly },
        set readOnly(value) { readOnly = Boolean(value) }
    }
    let activeIndex = null
    let activePointerId = null

    const activeTarget = () => activeIndex === null ? null : inputs()[activeIndex] ?? null
    const isReadOnly = () => root.dataset.readonly === "true"

    const valueFromPointer = (event, target) => {
        event.preventDefault()
        const rect = root.getBoundingClientRect()
        const vertical = root.dataset.orientation === "vertical"
        const rtl = getComputedStyle(root).direction === "rtl"
        let ratio = vertical
            ? (rect.bottom - event.clientY) / rect.height
            : (event.clientX - rect.left) / rect.width
        if (!vertical && rtl) ratio = 1 - ratio
        ratio = Math.max(0, Math.min(1, ratio))

        const minimum = Number(target.dataset.minimum)
        const maximum = Number(target.dataset.maximum)
        target.value = String(minimum + ratio * (maximum - minimum))
        target.dispatchEvent(new Event("input", { bubbles: true }))
    }

    const pointerDown = (event) => {
        if (isReadOnly()) {
            event.preventDefault()
            restoreReadOnlyValues()
            return
        }
        if (root.dataset.disabled === "true" || event.button !== 0 || event.isPrimary === false || activePointerId !== null) return
        const thumbs = inputs().filter((input) => !input.disabled)
        if (thumbs.length === 0) return

        const rect = root.getBoundingClientRect()
        const vertical = root.dataset.orientation === "vertical"
        const rtl = getComputedStyle(root).direction === "rtl"
        let ratio = vertical
            ? (rect.bottom - event.clientY) / rect.height
            : (event.clientX - rect.left) / rect.width
        if (!vertical && rtl) ratio = 1 - ratio
        ratio = Math.max(0, Math.min(1, ratio))

        const minimum = Number(thumbs[0].dataset.minimum)
        const maximum = Number(thumbs[0].dataset.maximum)
        const raw = minimum + ratio * (maximum - minimum)
        const target = thumbs.reduce((nearest, input) =>
            Math.abs(Number(input.value) - raw) < Math.abs(Number(nearest.value) - raw) ? input : nearest)
        activeIndex = inputs().indexOf(target)
        activePointerId = event.pointerId
        root.dataset.dragging = "true"
        root.setPointerCapture?.(event.pointerId)
        target.focus({ preventScroll: true })
        valueFromPointer(event, target)
    }

    const pointerMove = (event) => {
        if (isReadOnly()) {
            if (event.buttons !== 0) event.preventDefault()
            restoreReadOnlyValues()
            return
        }
        const target = activeTarget()
        if (target && activePointerId === event.pointerId) valueFromPointer(event, target)
    }

    const pointerUp = (event) => {
        if (activePointerId !== event.pointerId) return
        if (root.hasPointerCapture?.(event.pointerId)) root.releasePointerCapture(event.pointerId)
        activeIndex = null
        activePointerId = null
        delete root.dataset.dragging
    }

    const lostPointerCapture = (event) => {
        if (activePointerId !== event.pointerId) return
        activeIndex = null
        activePointerId = null
        delete root.dataset.dragging
    }

    const keyDown = (event) => {
        if (!isReadOnly()) return
        if (["ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", "PageUp", "PageDown", "Home", "End"].includes(event.key))
            event.preventDefault()
    }

    const input = (event) => {
        if (!isReadOnly()) return
        event.preventDefault()
        restoreReadOnlyValues()
    }

    root.addEventListener("pointerdown", pointerDown, true)
    root.addEventListener("pointermove", pointerMove, true)
    root.addEventListener("pointerup", pointerUp, true)
    root.addEventListener("pointercancel", pointerUp, true)
    root.addEventListener("keydown", keyDown)
    root.addEventListener("input", input, true)
    root.addEventListener("lostpointercapture", lostPointerCapture, true)
    Object.assign(state, { pointerDown, pointerMove, pointerUp, lostPointerCapture, keyDown, input, snapshotValues })
    listeners.set(root, state)
}

export function detach(root) {
    if (!root) return
    const listener = listeners.get(root)
    if (!listener) return
    if (typeof listener === "function") root.removeEventListener("keydown", listener)
    else {
        root.removeEventListener("pointerdown", listener.pointerDown, true)
        root.removeEventListener("pointermove", listener.pointerMove, true)
        root.removeEventListener("pointerup", listener.pointerUp, true)
        root.removeEventListener("pointercancel", listener.pointerUp, true)
        root.removeEventListener("lostpointercapture", listener.lostPointerCapture, true)
        root.removeEventListener("keydown", listener.keyDown)
        root.removeEventListener("input", listener.input, true)
        delete root.dataset.dragging
    }
    listeners.delete(root)
}
