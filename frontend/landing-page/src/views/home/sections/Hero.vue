<script setup lang="ts">
import AppButton from '@/components/ui/AppButton.vue'
import AppFlag from '@/components/ui/AppFlag.vue'
import { useWebAdmin } from '@/composables/useWebAdmin'

defineOptions({ name: 'HeroSection' })

const { registerUrl } = useWebAdmin()

const highlights = [
  { label: 'Programa Pionero', value: '20 cupos' },
  { label: 'Soporte Humano', value: 'WhatsApp' },
  { label: 'Primer Año', value: 'Gratis' },
  { label: 'Cobertura', value: 'RD' },
]
</script>

<template>
  <section class="relative w-full min-h-screen flex flex-col overflow-hidden bg-neutral">
    <div class="absolute inset-0 z-0">
      <img
        src="@/assets/img/hero/col-picture-desktop.webp"
        alt="Colmado dominicano digitalizado con TuColmadoRD"
        class="w-full h-full object-cover opacity-35 scale-105"
      />
      <div class="absolute inset-0 bg-linear-to-tr from-neutral via-neutral/60 to-transparent"></div>
    </div>

    <div class="relative z-10 flex-1 flex flex-col container mx-auto px-6 py-32">
      <div class="absolute right-0 top-48 w-full max-w-4xl flex justify-end reveal-right">
        <AppFlag side="right" variant="primary">
          Programa Pionero · 20 cupos
        </AppFlag>
      </div>

      <div class="mt-20 max-w-3xl reveal-left">
        <p class="text-primary font-black uppercase tracking-[0.3em] text-sm mb-6">
          Software para colmados dominicanos
        </p>

        <h1 class="text-5xl md:text-7xl lg:text-8xl font-bold text-neutral-content mb-8 leading-[0.85] tracking-tighter uppercase">
          IMPULSA <br />
          <span class="text-primary italic">TU NEGOCIO</span>
        </h1>

        <p class="text-xl text-neutral-content/70 mb-12 leading-relaxed max-w-xl font-medium border-l-4 border-secondary pl-8">
          La primera plataforma diseñada para el colmadero dominicano.
          Gestiona inventario, fiados, ventas y pedidos desde un solo lugar.
          Estamos en lanzamiento — únete a los primeros 20 colmados pioneros.
        </p>

        <div class="flex flex-wrap gap-4">
          <AppButton
            :href="registerUrl"
            size="lg"
            variant="primary"
            class="px-10 h-14 text-base rounded-none font-black uppercase tracking-widest"
          >
            Solicitar mi cupo
          </AppButton>
          <AppButton
            href="#como-funciona"
            size="lg"
            variant="outline"
            class="px-10 h-14 text-base text-neutral-content border-neutral-content hover:bg-neutral-content hover:text-neutral rounded-none font-black uppercase tracking-widest"
          >
            Ver cómo funciona
          </AppButton>
        </div>
      </div>

      <div class="mt-auto grid grid-cols-2 md:grid-cols-4 gap-8 bg-neutral/20 backdrop-blur-md p-8 -mx-6 md:mx-0 border-t border-neutral-content/10 reveal">
        <div
          v-for="item in highlights"
          :key="item.label"
          class="flex flex-col border-l-2 border-primary pl-5"
        >
          <span class="text-3xl md:text-4xl font-black text-neutral-content tracking-tighter italic">{{ item.value }}</span>
          <span class="text-xs uppercase font-bold tracking-[0.25em] text-primary mt-1">
            {{ item.label }}
          </span>
        </div>
      </div>
    </div>
  </section>
</template>
