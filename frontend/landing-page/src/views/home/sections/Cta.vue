<script setup lang="ts">
import { Icon } from '@iconify/vue'
import AppButton from '@/components/ui/AppButton.vue'
import { useWebAdmin } from '@/composables/useWebAdmin'

defineOptions({ name: 'CtaSection' })

const { registerUrl } = useWebAdmin()
</script>

<template>
  <section id="contacto" class="py-32 bg-secondary relative overflow-hidden">
    <div class="absolute inset-0 opacity-10">
      <div class="absolute top-0 left-0 w-96 h-96 bg-secondary-content rounded-full -translate-x-1/2 -translate-y-1/2 blur-3xl"></div>
      <div class="absolute bottom-0 right-0 w-96 h-96 bg-primary rounded-full translate-x-1/2 translate-y-1/2 blur-3xl"></div>
    </div>

    <div class="container mx-auto px-6 relative z-10">
      <div class="max-w-4xl mx-auto text-center reveal">
        <h2 class="text-5xl md:text-8xl font-black text-secondary-content mb-8 italic tracking-tighter uppercase leading-[0.85]">
          ¿LISTO PARA <br />
          <span class="text-neutral">DIGITALIZAR</span> <br />
          TU COLMADO?
        </h2>

        <p class="text-xl text-secondary-content/80 mb-12 max-w-2xl mx-auto font-medium">
          Quedan pocos cupos en el programa pionero.
          Sé parte de los primeros colmados en digitalizarse con TuColmadoRD.
        </p>

        <div class="flex flex-col md:flex-row justify-center gap-5">
          <AppButton
            :href="registerUrl"
            size="lg"
            variant="neutral"
            class="px-14 h-16 text-lg rounded-none font-black uppercase tracking-widest shadow-2xl hover:scale-105 transition-transform"
          >
            Solicitar mi cupo gratis
          </AppButton>
          <AppButton
            href="mailto:hola@tucolmadord.com"
            size="lg"
            variant="outline"
            class="px-14 h-16 text-lg text-secondary-content border-secondary-content hover:bg-secondary-content hover:text-secondary rounded-none font-black uppercase tracking-widest"
          >
            Contactar ventas
          </AppButton>
        </div>

        <div class="mt-14 flex justify-center gap-10 text-secondary-content/40">
          <div class="flex items-center gap-2">
            <Icon icon="lucide:shield-check" class="w-5 h-5" />
            <span class="text-xs font-bold uppercase tracking-widest">Seguro</span>
          </div>
          <div class="flex items-center gap-2">
            <Icon icon="lucide:zap" class="w-5 h-5" />
            <span class="text-xs font-bold uppercase tracking-widest">Rápido</span>
          </div>
          <div class="flex items-center gap-2">
            <Icon icon="lucide:map-pin" class="w-5 h-5" />
            <span class="text-xs font-bold uppercase tracking-widest">Hecho en RD</span>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>
