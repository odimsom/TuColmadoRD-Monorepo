<script setup lang="ts">
import { ref } from 'vue'
import { Icon } from '@iconify/vue'
import AppButton from '@/components/ui/AppButton.vue'
import AppLogo from '@/components/ui/AppLogo.vue'
import { useWebAdmin } from '@/composables/useWebAdmin'

const { loginUrl, registerUrl } = useWebAdmin()

const navLinks = [
  { label: 'Características', href: '#caracteristicas' },
  { label: 'Cómo funciona', href: '#como-funciona' },
  { label: 'Precios', href: '#precios' },
]

const isDim = ref(false)
const mobileOpen = ref(false)

function toggleTheme() {
  isDim.value = !isDim.value
  document.documentElement.setAttribute('data-theme', isDim.value ? 'dim' : 'tucolmadord')
}
</script>

<template>
  <nav class="sticky top-0 z-50 w-full pointer-events-none overflow-visible">
    <div class="flex items-start overflow-visible">
      <!-- Logo tab — clip parallelogram, always dark -->
      <div class="clip-logo-tab bg-base-200 h-20 w-auto min-w-[200px] flex items-center px-8 z-20 shadow-xl pointer-events-auto shrink-0">
        <a href="#" aria-label="TuColmadoRD inicio" class="flex items-center">
          <AppLogo />
        </a>
      </div>

      <!-- Nav bar -->
      <div class="flex-1 bg-neutral/90 backdrop-blur-md h-16 flex items-center justify-between px-8 -ml-8 z-10 pointer-events-auto relative">
        <!-- Desktop links -->
        <div class="absolute left-1/2 -translate-x-1/2 hidden lg:flex items-center gap-8">
          <a
            v-for="link in navLinks"
            :key="link.href"
            :href="link.href"
            class="text-neutral-content/70 font-bold text-xs hover:text-primary transition-colors uppercase tracking-[0.2em]"
          >
            {{ link.label }}
          </a>
        </div>

        <div class="ml-auto flex items-center gap-5">
          <!-- Theme toggle -->
          <button
            class="text-neutral-content/50 hover:text-neutral-content transition-colors"
            :aria-label="isDim ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro'"
            @click="toggleTheme"
          >
            <Icon :icon="isDim ? 'lucide:sun' : 'lucide:moon'" class="w-4 h-4" />
          </button>

          <!-- Desktop auth -->
          <div class="hidden sm:flex items-center gap-4">
            <a
              :href="loginUrl"
              class="text-neutral-content/80 font-bold text-xs uppercase tracking-widest hover:text-neutral-content transition-colors"
            >
              Iniciar Sesión
            </a>
            <AppButton :href="registerUrl" variant="primary" size="sm" class="rounded-none px-5 text-xs">
              Registrarse
            </AppButton>
          </div>

          <!-- Mobile menu toggle -->
          <button
            class="lg:hidden text-neutral-content/70 hover:text-neutral-content transition-colors"
            :aria-label="mobileOpen ? 'Cerrar menú' : 'Abrir menú'"
            @click="mobileOpen = !mobileOpen"
          >
            <Icon :icon="mobileOpen ? 'lucide:x' : 'lucide:menu'" class="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>

    <!-- Mobile menu -->
    <Transition name="slide-down">
      <div
        v-if="mobileOpen"
        class="lg:hidden bg-neutral/95 backdrop-blur-md border-b border-neutral-content/10 pointer-events-auto"
      >
        <div class="container mx-auto px-6 py-6 flex flex-col gap-4">
          <a
            v-for="link in navLinks"
            :key="link.href"
            :href="link.href"
            class="text-neutral-content/70 font-bold text-sm uppercase tracking-widest hover:text-primary transition-colors py-2 border-b border-neutral-content/5"
            @click="mobileOpen = false"
          >
            {{ link.label }}
          </a>
          <div class="flex gap-4 pt-2">
            <a :href="loginUrl" class="text-neutral-content/80 font-bold text-xs uppercase tracking-widest hover:text-neutral-content transition-colors">
              Iniciar Sesión
            </a>
            <AppButton :href="registerUrl" variant="primary" size="sm" class="rounded-none px-5 text-xs">
              Registrarse
            </AppButton>
          </div>
        </div>
      </div>
    </Transition>
  </nav>
</template>

<style scoped>
.slide-down-enter-active,
.slide-down-leave-active {
  transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.25s ease;
}
.slide-down-enter-from,
.slide-down-leave-to {
  transform: translateY(-8px);
  opacity: 0;
}
</style>
