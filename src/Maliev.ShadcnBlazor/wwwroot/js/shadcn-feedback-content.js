const observers = new WeakMap()
const toasters = new WeakMap()
const toasterStack = []
let globalToasterKeydown = null

function updateCarousel(root) {
  const viewport = root.querySelector('[data-slot="carousel-content"]')
  const track = root.querySelector('[data-slot="carousel-track"]')
  const selected = root.querySelector('[data-slot="carousel-item"][data-selected="true"]')
  if (!viewport || !track || !selected) return

  const vertical = root.dataset.orientation === 'vertical'
  const rtl = root.dataset.rtl === 'true'
  const align = root.dataset.align || 'start'
  let offset
  if (vertical) {
    offset = selected.offsetTop - track.offsetTop
    if (align === 'center') offset -= (viewport.clientHeight - selected.offsetHeight) / 2
    if (align === 'end') offset -= viewport.clientHeight - selected.offsetHeight
    track.style.translate = `0 ${-offset}px`
  } else {
    offset = rtl
      ? track.scrollWidth - (selected.offsetLeft - track.offsetLeft + selected.offsetWidth)
      : selected.offsetLeft - track.offsetLeft
    if (align === 'center') offset -= (viewport.clientWidth - selected.offsetWidth) / 2
    if (align === 'end') offset -= viewport.clientWidth - selected.offsetWidth
    track.style.translate = `${rtl ? offset : -offset}px 0`
  }
  track.dataset.measured = 'true'
}

export function syncCarousel(root) {
  updateCarousel(root)
  if (observers.has(root)) return

  const resize = new ResizeObserver(() => updateCarousel(root))
  resize.observe(root)
  const mutation = new MutationObserver(() => updateCarousel(root))
  mutation.observe(root, { subtree: true, attributes: true, childList: true, attributeFilter: ['data-selected'] })
  const viewport = root.querySelector('[data-slot="carousel-content"]')
  const capture = event => viewport?.setPointerCapture?.(event.pointerId)
  viewport?.addEventListener('pointerdown', capture)
  observers.set(root, { resize, mutation, viewport, capture })
}

export function detachCarousel(root) {
  const state = observers.get(root)
  if (!state) return
  state.resize.disconnect()
  state.mutation.disconnect()
  state.viewport?.removeEventListener('pointerdown', state.capture)
  observers.delete(root)
}

export function attachToaster(viewport, dotnet) {
  const visibility = () => dotnet.invokeMethodAsync('SetDocumentPaused', document.hidden)
  const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')
  const motion = () => dotnet.invokeMethodAsync('SetSystemReducedMotion', reducedMotion.matches)
  const capturePointer = event => {
    if (event.target.closest('button, a, input, select, textarea')) return
    event.target.closest('[data-slot="toast"]')?.setPointerCapture?.(event.pointerId)
  }
  toasterStack.push(viewport)
  if (!globalToasterKeydown) {
    globalToasterKeydown = event => {
      if (event.key !== 'F6' || toasterStack.length === 0) return
      event.preventDefault()
      const current = toasterStack.indexOf(document.activeElement)
      toasterStack[(current + 1) % toasterStack.length]?.focus()
    }
    document.addEventListener('keydown', globalToasterKeydown)
  }
  document.addEventListener('visibilitychange', visibility)
  reducedMotion.addEventListener('change', motion)
  viewport.addEventListener('pointerdown', capturePointer)
  visibility()
  motion()
  toasters.set(viewport, { visibility, reducedMotion, motion, capturePointer })
}

export function detachToaster(viewport) {
  const state = toasters.get(viewport)
  if (!state) return
  document.removeEventListener('visibilitychange', state.visibility)
  state.reducedMotion.removeEventListener('change', state.motion)
  viewport.removeEventListener('pointerdown', state.capturePointer)
  toasters.delete(viewport)
  const index = toasterStack.indexOf(viewport)
  if (index >= 0) toasterStack.splice(index, 1)
  if (toasterStack.length === 0 && globalToasterKeydown) {
    document.removeEventListener('keydown', globalToasterKeydown)
    globalToasterKeydown = null
  }
}
