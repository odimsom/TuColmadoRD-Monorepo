<script setup lang="ts">
import { Icon } from '@iconify/vue'
import AppButton from '@/components/ui/AppButton.vue'
import { useWebAdmin } from '@/composables/useWebAdmin'

defineOptions({ name: 'PricingSection' })

const { whatsappUrl } = useWebAdmin()

const perks = [
  {
    icon: 'lucide:calendar',
    title: '1 año de acceso completo',
    description: 'Sin límites en facturación, inventario ni fiados. Todo incluido hasta que evaluemos juntos el paso siguiente.',
  },
  {
    icon: 'lucide:message-circle',
    title: 'Soporte humano por WhatsApp',
    description: 'Hablas con nosotros, no con un bot. Lunes a sábado. Si hay problema, lo resolvemos ese mismo día.',
  },
  {
    icon: 'lucide:lightbulb',
    title: 'Tu voz define el producto',
    description: 'Los primeros 20 clientes deciden qué se construye. Si necesitas una función y tiene sentido, la hacemos.',
  },
  {
    icon: 'lucide:shield-check',
    title: 'Sin tarjeta de crédito',
    description: 'No pedimos forma de pago ahora. Al completar el año evaluamos juntos, con precios preferenciales para los pioneros.',
  },
]

const plans = [
  {
    name: 'Básico',
    price: '1,200',
    description: 'Para el colmado que quiere arrancar en digital.',
    features: [
      'Inventario ilimitado',
      'Punto de venta (POS)',
      'Control de fiados',
      'Reportes básicos',
      '2 usuarios',
      'Soporte por chat',
    ],
    variant: 'outline' as const,
    highlight: false,
  },
  {
    name: 'Profesional',
    price: '2,200',
    description: 'El plan completo para crecer en serio.',
    features: [
      'Todo lo del plan Básico',
      'App de pedidos para clientes',
      'Gestión de delivery',
      'Reportes avanzados',
      'Hasta 5 usuarios',
      'Pagos digitales',
      'Soporte prioritario WhatsApp',
    ],
    variant: 'primary' as const,
    highlight: true,
  },
  {
    name: 'Empresarial',
    price: '3,800',
    description: 'Para mini-supermercados y colmados grandes.',
    features: [
      'Todo lo del plan Profesional',
      'Usuarios ilimitados',
      'Multi-sucursal',
      'API de integración',
      'Reportes personalizados',
      'Gestor de cuenta dedicado',
    ],
    variant: 'outline' as const,
    highlight: false,
  },
]
</script>

<template>
  <section id="precios" class="py-32 bg-base-100 scroll-mt-20">
    <div class="container mx-auto px-6">

      <!-- Programa Pionero -->
      <div class="text-center mb-16 reveal">
        <span class="inline-block px-4 py-1.5 bg-primary/10 text-primary text-xs font-black uppercase tracking-[0.2em] mb-6">
          Programa Pionero · Activo ahora
        </span>
        <h2 class="text-6xl md:text-8xl font-black text-base-content mb-4 tracking-tighter uppercase leading-[0.85] italic">
          RD$0
        </h2>
        <p class="text-base-content/50 text-sm font-bold uppercase tracking-widest">
          Primer año completo · 20 cupos disponibles · Por orden de llegada
        </p>
      </div>

      <div class="max-w-3xl mx-auto relative mb-32 reveal">
        <div class="absolute -top-5 left-1/2 -translate-x-1/2 px-6 py-2 bg-primary text-primary-content text-xs font-black uppercase tracking-widest shadow-lg z-10">
          Cupos limitados
        </div>

        <div class="bg-base-200 border-2 border-primary/30 shadow-xl p-10 md:p-14">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
            <div v-for="perk in perks" :key="perk.title" class="flex items-start gap-4">
              <div class="w-10 h-10 bg-primary text-primary-content flex items-center justify-center shrink-0">
                <Icon :icon="perk.icon" class="w-5 h-5" />
              </div>
              <div>
                <h4 class="font-black text-base-content text-sm mb-1">{{ perk.title }}</h4>
                <p class="text-base-content/50 text-xs leading-relaxed">{{ perk.description }}</p>
              </div>
            </div>
          </div>

          <AppButton
            :href="whatsappUrl"
            variant="primary"
            size="lg"
            wide
            class="rounded-none font-black uppercase tracking-widest text-sm"
          >
            Escríbenos por WhatsApp, es gratis
          </AppButton>

          <p class="text-center text-base-content/30 text-xs mt-6 uppercase tracking-widest">
            Se asignan por orden de postulación · 20 plazas
          </p>
        </div>
      </div>

      <!-- Planes regulares -->
      <div class="text-center mb-16 reveal">
        <span class="inline-block px-4 py-1.5 bg-base-300 text-base-content/60 text-xs font-black uppercase tracking-[0.2em] mb-6">
          Cuando se agoten los cupos
        </span>
        <h3 class="text-4xl md:text-5xl font-black text-base-content tracking-tighter uppercase leading-tight">
          Planes que caben<br />
          <span class="text-primary italic">en cualquier colmado</span>
        </h3>
        <p class="text-base-content/40 mt-4 text-sm">Precios en pesos dominicanos · IVA incluido</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-5xl mx-auto">
        <div
          v-for="plan in plans"
          :key="plan.name"
          :class="[
            'flex flex-col p-8 reveal border-2',
            plan.highlight
              ? 'border-primary bg-primary/5 relative'
              : 'border-base-300 bg-base-100'
          ]"
        >
          <div v-if="plan.highlight" class="absolute -top-4 left-1/2 -translate-x-1/2 px-4 py-1 bg-primary text-primary-content text-xs font-black uppercase tracking-widest">
            Más popular
          </div>

          <div class="mb-6">
            <h4 class="font-black text-base-content uppercase tracking-widest text-sm mb-2">{{ plan.name }}</h4>
            <div class="flex items-baseline gap-1 mb-2">
              <span class="text-xs text-base-content/40 font-bold">RD$</span>
              <span class="text-4xl font-black text-base-content tracking-tighter">{{ plan.price }}</span>
              <span class="text-base-content/40 text-xs">/mes</span>
            </div>
            <p class="text-base-content/50 text-xs leading-relaxed">{{ plan.description }}</p>
          </div>

          <ul class="space-y-2 mb-8 flex-1">
            <li v-for="f in plan.features" :key="f" class="flex items-center gap-2 text-xs text-base-content/70">
              <Icon icon="lucide:check" :class="['w-3.5 h-3.5 shrink-0', plan.highlight ? 'text-primary' : 'text-base-content/40']" />
              {{ f }}
            </li>
          </ul>

          <AppButton
            :href="whatsappUrl"
            :variant="plan.highlight ? 'primary' : 'outline'"
            class="rounded-none font-black uppercase tracking-widest text-xs w-full"
          >
            Consultar
          </AppButton>
        </div>
      </div>

      <p class="text-center text-base-content/30 text-xs mt-10 reveal">
        Los colmados del programa pionero tendrán precios preferenciales al finalizar el primer año.
      </p>
    </div>
  </section>
</template>
