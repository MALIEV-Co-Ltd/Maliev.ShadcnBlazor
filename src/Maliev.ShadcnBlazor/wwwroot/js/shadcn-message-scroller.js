export function attach(root, dotnet) {
  const viewport = root.querySelector('[data-slot="message-scroller-viewport"]')
  const content = root.querySelector('[data-slot="message-scroller-content"]')
  if (!viewport || !content) throw new Error('MessageScroller requires viewport and content')
  let disposed = false
  let frame = 0
  let sequence = 0
  let autoscrollTimer = 0
  let programmaticFocus = false
  const rows = () => [...content.querySelectorAll(':scope > [data-slot="message-scroller-item"]')]
  const snapshot = () => ({
    scrollTop: viewport.scrollTop,
    viewportHeight: viewport.clientHeight,
    contentHeight: Math.max(viewport.scrollHeight, content.scrollHeight),
    items: rows().map(x => ({ messageId: x.dataset.messageId, top: x.offsetTop, height: x.offsetHeight, scrollAnchor: x.dataset.scrollAnchor === 'true' })),
    sequence: ++sequence,
    preserveScrollOnPrepend: viewport.dataset.preserveScrollOnPrepend === 'true'
  })
  const report = () => {
    if (disposed) return
    cancelAnimationFrame(frame)
    frame = requestAnimationFrame(() => dotnet.invokeMethodAsync('OnScrollerMeasurementAsync', snapshot()))
  }
  const intent = () => dotnet.invokeMethodAsync('OnScrollerUserIntentAsync')
  const selectionIntent = () => { const selection = document.getSelection(); if (selection?.anchorNode && viewport.contains(selection.anchorNode)) intent() }
  const keyIntent = event => { if (['ArrowDown','ArrowUp','End','Home','PageDown','PageUp',' '].includes(event.key)) intent() }
  const resizeObserver = new ResizeObserver(report)
  resizeObserver.observe(viewport); resizeObserver.observe(content)
  const intersectionObserver = new IntersectionObserver(report, { root: viewport })
  rows().forEach(row => intersectionObserver.observe(row))
  const mutationObserver = new MutationObserver(() => { intersectionObserver.disconnect(); rows().forEach(row => intersectionObserver.observe(row)); report() })
  mutationObserver.observe(content, { childList: true, subtree: false })
  viewport.addEventListener('scroll', report, { passive: true })
  viewport.addEventListener('wheel', intent, { passive: true })
  viewport.addEventListener('touchstart', intent, { passive: true })
  viewport.addEventListener('pointerdown', intent, { passive: true })
  viewport.addEventListener('keydown', keyIntent)
  const focusIntent = () => { if (!programmaticFocus) intent() }
  viewport.addEventListener('focusin', focusIntent)
  document.addEventListener('selectionchange', selectionIntent)
  report()
  return {
    measure: snapshot,
    refresh: report,
    scrollTo(top, behavior, focusViewport = false) {
      clearTimeout(autoscrollTimer)
      if (behavior === 'smooth') { root.setAttribute('data-autoscrolling', ''); viewport.setAttribute('data-autoscrolling', '') }
      viewport.scrollTo({ top, behavior })
      if (focusViewport) { programmaticFocus = true; viewport.focus({ preventScroll: true }); programmaticFocus = false }
      autoscrollTimer = setTimeout(() => { root.removeAttribute('data-autoscrolling'); viewport.removeAttribute('data-autoscrolling') }, behavior === 'smooth' ? 500 : 0)
    },
    dispose() {
      disposed = true; cancelAnimationFrame(frame); clearTimeout(autoscrollTimer); resizeObserver.disconnect(); intersectionObserver.disconnect(); mutationObserver.disconnect()
      viewport.removeEventListener('scroll', report); viewport.removeEventListener('wheel', intent); viewport.removeEventListener('touchstart', intent); viewport.removeEventListener('pointerdown', intent); viewport.removeEventListener('keydown', keyIntent); viewport.removeEventListener('focusin', focusIntent); document.removeEventListener('selectionchange', selectionIntent)
    }
  }
}
