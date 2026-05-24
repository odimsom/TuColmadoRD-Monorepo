<script setup lang="ts">
import { Icon } from '@iconify/vue'
import AppButton from '@/components/ui/AppButton.vue'
import { useWebAdmin } from '@/composables/useWebAdmin'

defineOptions({ name: 'PricingSection' })

const { registerUrl } = useWebAdmin()

const perks = [
  {
    icon: 'lucide:calendar',
    title: '1 año de acceso completo',
    description: 'Sin límites de usuarios, facturación, inventario ni fiados. Todo incluido hasta que evaluemos juntos el siguiente paso.',
  },
  {
    icon: 'lucide:headphones',
    title: 'Soporte humano directo',
    description: 'Hablas con nosotros, no con un bot. Lunes a sábado. Entendemos tu negocio porque somos de aquí.',
  },
  {
    icon: 'lucide:lightbulb',
    title: 'Tu voz define el producto',
    description: 'Nuestros primeros 20 clientes deciden qué se construye. Si necesitas una función y tiene sentido, la hacemos.',
  },
  {
    icon: 'lucide:shield-check',
    title: 'Sin tarjeta de crédito',
    description: 'No pedimos forma de pago ahora. Al completar el año evaluamos juntos, con precios preferenciales para los pioneros.',
  },
]
</script>

<template>
  <section id="precios" class="py-32 bg-base-200 scroll-mt-20">
    <div class="container mx-auto px-6">
      <div class="text-center mb-16 reveal">
        <span class="inline-block px-4 py-1.5 bg-primary/10 text-primary text-xs font-black uppercase tracking-[0.2em] mb-6">
          Programa Pionero
        </span>
        <h2 class="text-6xl md:text-8xl font-black text-base-content mb-4 tracking-tighter uppercase leading-[0.85] italic">
          RD$0
        </h2>
        <p class="text-base-content/50 text-sm font-bold uppercase tracking-widest">
          Primer año · 20 cupos disponibles
        </p>
      </div>

      <div class="max-w-3xl mx-auto relative">
        <!-- Badge -->
        <div class="absolute -top-5 left-1/2 -translate-x-1/2 px-6 py-2 bg-primary text-primary-content text-xs font-black uppercase tracking-widest shadow-lg z-10">
          Cupos limitados
        </div>

        <div class="bg-base-100 border-2 border-primary/30 shadow-xl p-10 md:p-14 reveal">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
            <div
              v-for="perk in perks"
              :key="perk.title"
              class="flex items-start gap-4"
            >
              <div class="w-10 h-10 bg-primary text-primary-content flex items-center justify-center shrink-0 rounded-none">
                <Icon :icon="perk.icon" class="w-5 h-5" />
              </div>
              <div>
                <h4 class="font-black text-base-content text-sm mb-1">{{ perk.title }}</h4>
                <p class="text-base-content/50 text-xs leading-relaxed">{{ perk.description }}</p>
              </div>
            </div>
          </div>

          <AppButton
            :href="registerUrl"
            variant="primary"
            size="lg"
            wide
            class="rounded-none font-black uppercase tracking-widest text-sm h-14"
          >
            Solicitar mi lugar como pionero
          </AppButton>

          <p class="text-center text-base-content/30 text-xs mt-6 font-medium uppercase tracking-widest">
            Se asignan por orden de postulación · Plazas limitadas a 20
          </p>
        </div>
      </div>
    </div>
  </section>
</template>
