<script setup lang="ts">
import { Icon } from '@iconify/vue'
import AppButton from '@/components/ui/AppButton.vue'
import { useWebAdmin } from '@/composables/useWebAdmin'

defineOptions({ name: 'HowItWorksSection' })

const { whatsappUrl } = useWebAdmin()

const steps = [
  {
    number: '01',
    icon: 'lucide:user-plus',
    title: 'Solicita tu cupo',
    description: 'Completa el formulario en menos de 2 minutos. Cuéntanos sobre tu colmado y te contactamos por WhatsApp para coordinar tu acceso.',
  },
  {
    number: '02',
    icon: 'lucide:settings',
    title: 'Configuramos juntos',
    description: 'Un especialista nuestro te ayuda a configurar tu inventario inicial, tus empleados y tus primeras categorías de productos.',
  },
  {
    number: '03',
    icon: 'lucide:trending-up',
    title: 'Empieza a vender mejor',
    description: 'Lleva tus ventas, fiados, inventario y pedidos desde tu celular o computadora. Sin papeles, sin cálculos manuales.',
  },
]
</script>

<template>
  <section id="como-funciona" class="py-32 bg-base-100 scroll-mt-20">
    <div class="container mx-auto px-6">
      <div class="text-center mb-20 reveal">
        <span class="inline-block px-4 py-1.5 bg-secondary/10 text-secondary text-xs font-black uppercase tracking-[0.2em] mb-6">
          Proceso simple
        </span>
        <h2 class="text-5xl md:text-7xl font-black text-base-content tracking-tighter uppercase leading-[0.85] italic">
          Tres pasos<br />
          <span class="text-secondary">y listo</span>
        </h2>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-0 max-w-5xl mx-auto relative">
        <!-- Connecting line (desktop) -->
        <div class="absolute top-12 left-[16.66%] right-[16.66%] h-px bg-base-300 hidden md:block"></div>

        <div
          v-for="(step, index) in steps"
          :key="step.number"
          class="flex flex-col items-center text-center p-8 reveal"
          :style="{ transitionDelay: `${index * 150}ms` }"
        >
          <div class="relative mb-8">
            <div class="w-24 h-24 bg-base-200 border-2 border-base-300 flex items-center justify-center relative z-10">
              <Icon :icon="step.icon" class="w-8 h-8 text-primary" />
            </div>
            <span class="absolute -top-3 -right-3 text-xs font-black text-primary bg-base-100 border border-primary/30 px-2 py-1 z-20">
              {{ step.number }}
            </span>
          </div>
          <h3 class="text-xl font-black text-base-content uppercase tracking-tight mb-4">
            {{ step.title }}
          </h3>
          <p class="text-base-content/50 text-sm leading-relaxed">
            {{ step.description }}
          </p>
        </div>
      </div>

      <div class="text-center mt-16 reveal">
        <AppButton
          :href="whatsappUrl"
          variant="primary"
          size="lg"
          class="rounded-none font-black uppercase tracking-widest px-12"
        >
          Empezar ahora
        </AppButton>
      </div>
    </div>
  </section>
</template>
