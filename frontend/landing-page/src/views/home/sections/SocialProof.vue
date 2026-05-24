<script setup lang="ts">
import { Icon } from '@iconify/vue'

defineOptions({ name: 'SocialProofSection' })

const taken = 18
const total = 20
const pct = (taken / total) * 100

const testimonials = [
  {
    initials: 'JR',
    bgClass: 'bg-primary',
    name: 'Joel Reyes',
    colmado: 'Colmado Reyes · Santiago',
    quote: 'Fui el número 4 en entrar. Empecé sin saber prender una computadora y en 3 días ya cobraba con el celular. El equipo me ayudó por WhatsApp hasta que lo dominé.',
  },
  {
    initials: 'AM',
    bgClass: 'bg-secondary',
    name: 'Doña Altagracia',
    colmado: 'Colmado La Bendición · Higüey',
    quote: 'Entré al programa hace 2 meses. Antes perdía dinero en fiados que no anotaba. Ahora todo está en la pantalla y ya nadie se me escapa. ¡No esperen a que se llenen los cupos!',
  },
  {
    initials: 'CD',
    bgClass: 'bg-neutral',
    name: 'Carlos Díaz',
    colmado: 'Mini-market Don Carlos · Santo Domingo',
    quote: 'Me alegra haber tomado uno de los primeros cupos. Vinieron a mi colmado y pasaron todo mi cuaderno al sistema en una tarde. Es el cambio que necesitaba mi negocio.',
  },
]
</script>

<template>
  <section class="py-32 bg-base-100">
    <div class="container mx-auto px-6">

      <!-- Counter heading -->
      <div class="text-center mb-12 reveal">
        <div class="flex justify-center mb-6">
          <span class="inline-flex items-center gap-2 px-3.5 py-1.5 bg-primary/10 text-primary text-[11px] font-black uppercase tracking-[0.20em]">
            <span class="relative flex h-2 w-2">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary opacity-75"></span>
              <span class="relative inline-flex rounded-full h-2 w-2 bg-primary"></span>
            </span>
            Cupos del programa pionero
          </span>
        </div>
        <h2 class="text-5xl md:text-7xl font-black text-base-content tracking-tighter leading-[0.85] italic mt-6 mb-4">
          SOLO QUEDAN <span class="text-secondary">{{ total - taken }}</span>
          <span class="not-italic block md:inline"> DISPONIBLES</span>
        </h2>

        <!-- Progress bar -->
        <div class="max-w-xl mx-auto mt-10">
          <div class="h-4 bg-base-300 relative overflow-hidden">
            <div class="absolute inset-0 h-full" :style="{ width: pct + '%', background: 'linear-gradient(90deg, var(--color-primary), var(--color-secondary))' }"></div>
          </div>
          <div class="flex justify-between mt-3 text-[12px] font-black uppercase tracking-[0.18em] text-base-content/60">
            <span>{{ taken }} de {{ total }} ocupados</span>
            <span class="text-secondary">¡Casi lleno!</span>
          </div>
        </div>

        <p class="mt-8 text-[13px] font-bold text-base-content/40 italic">
          Último cupo asignado hace 42 minutos a un colmado en La Romana.
        </p>
      </div>

      <!-- Testimonials -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-7xl mx-auto mt-20">
        <figure v-for="t in testimonials" :key="t.name"
          class="m-0 bg-base-100 border border-base-300 p-7 pb-6 flex flex-col reveal">
          <Icon icon="lucide:quote" class="w-7 h-7 text-secondary shrink-0" />
          <blockquote class="m-0 mt-3.5 mb-6 text-[15px] leading-relaxed text-base-content flex-1">
            {{ t.quote }}
          </blockquote>
          <figcaption class="flex gap-3.5 items-center border-t border-base-300 pt-4.5">
            <div :class="['w-12 h-12 text-white grid place-items-center font-serif font-black text-base rounded-full shrink-0', t.bgClass]">
              {{ t.initials }}
            </div>
            <div>
              <div class="text-[13px] font-black text-base-content">{{ t.name }}</div>
              <div class="text-[11px] text-base-content/40 mt-0.5">{{ t.colmado }}</div>
            </div>
          </figcaption>
        </figure>
      </div>

    </div>
  </section>
</template>
