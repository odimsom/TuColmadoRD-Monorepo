<script setup lang="ts">
import { useWebAdmin } from '../composables/useWebAdmin'

const { registerUrl } = useWebAdmin()

const features = [
  {
    title: '1 año de acceso completo',
    description: 'Sin límites de usuarios, facturación, inventario o fiados. Todo incluido hasta Abril 2027.'
  },
  {
    title: 'Soporte dominicano por WhatsApp',
    description: 'Hablas con nosotros, no con un bot. Lunes a sábado. Entendemos tu colmado porque somos de aquí.'
  },
  {
    title: 'Tu voz define el producto',
    description: 'Nuestros primeros 20 clientes deciden qué se construye. Si sugieres una feature y es necesaria, la hacemos.'
  },
  {
    title: 'Sin tarjeta de crédito ahora',
    description: 'No pedimos forma de pago. Al completar el año, evaluaremos juntos el siguiente paso con precios preferenciales.'
  }
]
</script>

<template>
  <section id="precios" class="max-w-4xl mx-auto mb-48 pt-20 scroll-mt-32">
    <div class="text-center mb-16 px-8">
      <span class="px-4 py-1.5 rounded-full border border-blue-500/30 bg-blue-500/5 text-blue-400 text-xs font-black uppercase tracking-[0.2em] mb-6 inline-block">TU INVERSIÓN EN TI</span>
      <h2 class="text-6xl md:text-8xl font-bebas text-white mb-10 uppercase tracking-widest leading-none">RD$0</h2>
    </div>

    <div class="relative p-12 md:p-16 rounded-[3rem] bg-gradient-to-br from-blue-500/10 via-slate-900/80 to-slate-900/80 border-2 border-blue-500/50 shadow-[0_0_60px_rgba(37,99,235,0.2)]">
      <div class="absolute -top-6 left-1/2 -translate-x-1/2 px-6 py-2 bg-blue-500 text-white rounded-full text-sm font-black uppercase tracking-[0.2em] shadow-xl">
        PROGRAMA PILOTO
      </div>

      <h3 class="text-4xl md:text-5xl font-bebas text-white mb-4 uppercase tracking-wider">Usuario Pionero</h3>
      <p class="text-lg text-slate-300 mb-12 font-medium">
        Buscamos a los primeros 20 clientes pioneros para construir TuColmadoRD juntos.
      </p>

      <div class="space-y-8 mb-16">
        <div v-for="feature in features" :key="feature.title" class="flex items-start gap-6">
          <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center flex-shrink-0 text-white font-black text-xl shadow-lg shadow-blue-500/50">
            ✓
          </div>
          <div>
            <h4 class="text-xl font-black text-white mb-2">{{ feature.title }}</h4>
            <p class="text-slate-400 text-sm">{{ feature.description }}</p>
          </div>
        </div>
      </div>

      <a :href="registerUrl" class="block w-full py-5 text-center bg-blue-600 hover:bg-blue-500 text-white font-black rounded-xl transition-all text-sm uppercase tracking-widest shadow-[0_10px_30px_rgba(37,99,235,0.3)]">
        Solicitar mi lugar
      </a>

      <p class="text-center text-slate-500 text-xs mt-8 font-medium">
        Plazas limitadas a los primeros 20 clientes. Se asignan por orden de postulación.
      </p>
    </div>
  </section>
</template>
