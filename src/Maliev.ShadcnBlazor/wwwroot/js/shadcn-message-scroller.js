export function attach(root, dotnet) {
  const viewport = root.querySelector('[data-slot="message-scroller-viewport"]')
  const content = root.querySelector('[data-slot="message-scroller-content"]')
  if (!viewport || !content) throw new Error('MessageScroller requires viewport and content')
  let disposed = false
  let frame = 0
  let sequence = 0
  let autoscrollTimer = 0
  let programmaticFocus = false
  let pendingUserMeasurement = null
  const scrollWaiters = new Set()
  const rows = () => [...content.querySelectorAll(':scope > [data-slot="message-scroller-item"]')]
  const snapshot = () => ({
    scrollTop: viewport.scrollTop,
    viewportHeight: viewport.clientHeight,
    contentHeight: Math.max(viewport.scrollHeight, content.scrollHeight),
    items: rows().map(x => ({ messageId: x.dataset.messageId, top: x.offsetTop, height: x.offsetHeight, scrollAnchor: x.dataset.scrollAnchor === 'true' })),
    sequence: ++sequence,
    preserveScrollOnPrepend: viewport.dataset.preserveScrollOnPrepend === 'true'
  })
  const report = (userScroll = false) => {
    if (disposed) return
    if (userScroll) pendingUserMeasurement = snapshot()
    cancelAnimationFrame(frame)
    frame = requestAnimationFrame(async () => {
      const next = snapshot()
      const userMeasurement = pendingUserMeasurement
      pendingUserMeasurement = null
      if (userMeasurement) await dotnet.invokeMethodAsync('OnScrollerUserScrollAsync', userMeasurement.scrollTop, userMeasurement.viewportHeight, userMeasurement.contentHeight, userMeasurement.sequence)
      await dotnet.invokeMethodAsync('OnScrollerMeasurementAsync', next)
    })
  }
  const intent = () => report(true)
  const selectionIntent = () => { const selection = document.getSelection(); if (selection?.anchorNode && viewport.contains(selection.anchorNode)) intent() }
  const keyIntent = event => { if (['ArrowDown','ArrowUp','End','Home','PageDown','PageUp',' '].includes(event.key)) intent() }
  const resizeObserver = new ResizeObserver(() => report())
  resizeObserver.observe(viewport); resizeObserver.observe(content)
  const intersectionObserver = new IntersectionObserver(() => report(), { root: viewport })
  rows().forEach(row => intersectionObserver.observe(row))
  const mutationObserver = new MutationObserver(() => { intersectionObserver.disconnect(); rows().forEach(row => intersectionObserver.observe(row)); report() })
  mutationObserver.observe(content, { childList: true, subtree: false })
  const scroll = () => report()
  viewport.addEventListener('scroll', scroll, { passive: true })
  viewport.addEventListener('wheel', intent, { passive: true })
  viewport.addEventListener('touchstart', intent, { passive: true })
  viewport.addEventListener('touchend', intent, { passive: true })
  viewport.addEventListener('pointerdown', intent, { passive: true })
  viewport.addEventListener('pointerup', intent, { passive: true })
  viewport.addEventListener('keydown', keyIntent)
  const focusIntent = () => { if (!programmaticFocus) intent() }
  const waitForScroll = (top, behavior) => new Promise(resolve => {
    const target = Math.max(0, Math.min(top, viewport.scrollHeight - viewport.clientHeight))
    let animationFrame = 0
    let timeout = 0
    let finished = false
    const finish = () => {
      if (finished) return
      finished = true
      cancelAnimationFrame(animationFrame)
      clearTimeout(timeout)
      viewport.removeEventListener('scrollend', check)
      scrollWaiters.delete(finish)
      resolve()
    }
    const check = () => {
      if (Math.abs(viewport.scrollTop - target) <= 1) { finish(); return }
      animationFrame = requestAnimationFrame(check)
    }
    scrollWaiters.add(finish)
    viewport.addEventListener('scrollend', check)
    timeout = setTimeout(finish, behavior === 'smooth' ? 1200 : 250)
    animationFrame = requestAnimationFrame(check)
  })
  viewport.addEventListener('focusin', focusIntent)
  document.addEventListener('selectionchange', selectionIntent)
  report()
  return {
    measure: snapshot,
    refresh: report,
    scrollTo(top, behavior, focusViewport = false) {
      clearTimeout(autoscrollTimer)
      root.setAttribute('data-autoscrolling', ''); viewport.setAttribute('data-autoscrolling', '')
      const completion = waitForScroll(top, behavior)
      viewport.scrollTo({ top, behavior })
      if (focusViewport) { programmaticFocus = true; viewport.focus({ preventScroll: true }); programmaticFocus = false }
      autoscrollTimer = setTimeout(() => { root.removeAttribute('data-autoscrolling'); viewport.removeAttribute('data-autoscrolling') }, behavior === 'smooth' ? 500 : 100)
      return completion
    },
    dispose() {
      disposed = true; cancelAnimationFrame(frame); clearTimeout(autoscrollTimer); scrollWaiters.forEach(finish => finish()); resizeObserver.disconnect(); intersectionObserver.disconnect(); mutationObserver.disconnect()
      viewport.removeEventListener('scroll', scroll); viewport.removeEventListener('wheel', intent); viewport.removeEventListener('touchstart', intent); viewport.removeEventListener('touchend', intent); viewport.removeEventListener('pointerdown', intent); viewport.removeEventListener('pointerup', intent); viewport.removeEventListener('keydown', keyIntent); viewport.removeEventListener('focusin', focusIntent); document.removeEventListener('selectionchange', selectionIntent)
    }
  }
}
