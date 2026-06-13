import { onMounted, onBeforeUnmount } from 'vue'

/**
 * Activa las animaciones de entrada: observa todos los elementos con
 * .reveal / .reveal-left / .reveal-right y les agrega .active cuando
 * entran al viewport. Sin esto, el CSS los deja en opacity:0 para siempre.
 */
export function useReveal() {
  let observer: IntersectionObserver | null = null

  onMounted(() => {
    const targets = document.querySelectorAll<HTMLElement>('.reveal, .reveal-left, .reveal-right')

    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    if (reduced || !('IntersectionObserver' in window)) {
      targets.forEach(el => el.classList.add('active'))
      return
    }

    observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add('active')
            observer?.unobserve(entry.target)
          }
        }
      },
      { threshold: 0.15, rootMargin: '0px 0px -40px 0px' },
    )
    targets.forEach(el => observer!.observe(el))
  })

  onBeforeUnmount(() => observer?.disconnect())
}
