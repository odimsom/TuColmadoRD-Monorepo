<script setup lang="ts">
import AppSection from '@/components/ui/AppSection.vue'

defineOptions({ name: 'FeaturesSection' })

const sections = [
  {
    image: '/src/assets/img/hero/col-picture-mobile.webp',
    imageSide: 'left' as const,
    flagText: 'Control Total',
    flagVariant: 'secondary' as const,
    title: 'Gestión de',
    highlight: 'Inventario',
    description: 'Digitaliza tu stock y olvídate de las mermas. Nuestra tecnología te permite llevar un control exacto de cada producto que entra y sale de tu colmado.',
    features: [
      { title: 'Stock en Tiempo Real', icon: 'lucide:package-check' },
      { title: 'Alertas de Vencimiento', icon: 'lucide:clock' },
      { title: 'Reportes de Ventas', icon: 'lucide:bar-chart' },
      { title: 'Multi-dispositivo', icon: 'lucide:smartphone' }
    ],
    benefits: [
      'Reducción de pérdidas en un 30%',
      'Cierre de caja en 5 minutos',
      'Historial de compras detallado'
    ]
  },
  {
    image: '/src/assets/img/hero/col-picture-desktop.webp',
    imageSide: 'right' as const,
    flagText: 'Más Ventas',
    flagVariant: 'primary' as const,
    title: 'Pedidos que',
    highlight: 'No Paran',
    description: 'Llega a más clientes sin esfuerzo. Tu colmado ahora está disponible 24/7 para que tus vecinos puedan pedir desde la comodidad de su hogar.',
    features: [
      { title: 'WhatsApp Integration', icon: 'lucide:message-circle' },
      { title: 'App para Clientes', icon: 'lucide:app-window' },
      { title: 'Gestión de Delivery', icon: 'lucide:truck' },
      { title: 'Pagos Digitales', icon: 'lucide:credit-card' }
    ],
    benefits: [
      'Aumento de pedidos en un 50%',
      'Fidelización de clientes',
      'Menos errores en las órdenes'
    ]
  }
]
</script>

<template>
  <section id="caracteristicas" class="bg-base-100 py-32">
    <AppSection
      v-for="(section, index) in sections"
      :key="index"
      v-bind="section"
    />
  </section>
</template>
