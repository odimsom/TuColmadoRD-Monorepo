<script setup lang="ts">
import { ref } from 'vue'

const activeFaq = ref<number | null>(null)

const toggleFaq = (index: number) => {
  activeFaq.value = activeFaq.value === index ? null : index
}

const faqs = [
  { p: '¿Necesito internet para usar el sistema?', r: 'No. TuColmadoRD funciona sin internet. Puedes registrar ventas, fiados e inventario aunque se vaya la luz o el WIFI. Cuando vuelva la conexión, todo se sincroniza automáticamente.' },
  { p: '¿Cuánto tiempo tarda configurar el sistema?', r: 'La mayoría de colmaderos están operando el mismo día. El proceso es: crear cuenta → agregar productos → abrir turno → empezar a vender. Si tienes una lista en Excel, lo importamos por ti sin costo.' },
  { p: '¿Puedo tener más de un empleado usando el sistema?', r: 'Sí. Creas usuarios para cada cajero. El sistema registra quién hizo cada venta. El dueño ve todo desde su celular en tiempo real.' },
  { p: '¿Funciona en tablets o solo en computadora?', r: 'Funciona en cualquier dispositivo: computadora, tablet Android o iPad. El POS está optimizado para pantallas táctiles. Muchos colmaderos lo usan en una tablet de RD$8,000.' },
  { p: '¿Qué pasa si cancelo? ¿Pierdo mis datos?', r: 'No. Tienes 30 días para exportar toda tu información (ventas, clientes, inventario) después de cancelar. Tus datos siempre son tuyos.' },
  { p: '¿El sistema maneja los fiados (la libreta)?', r: "Sí, es una de las funciones más usadas. Registras fiados por cliente, el sistema calcula el balance y avisa cuando alguien lleva mucho tiempo sin pagar. Adiós a la libreta física y a los 'no sé cuánto me debes'." },
  { p: '¿Tienen soporte en español dominicano?', r: 'Claro que sí. Soporte 100% dominicano por WhatsApp, de lunes a sábado. Entendemos cómo funciona un colmado porque somos de aquí.' }
]
</script>

<template>
  <section id="faq" class="max-w-3xl mx-auto mb-48 pt-20 scroll-mt-32">
    <div class="text-center mb-16">
      <h2 class="text-6xl md:text-8xl font-bebas text-white mb-10 uppercase tracking-widest">Preguntas frecuentes</h2>
    </div>

    <div class="space-y-4 mb-20">
      <div
        v-for="(faq, i) in faqs"
        :key="i"
        class="rounded-2xl border transition-all duration-300"
        :class="activeFaq === i ? 'bg-[#0D1B2E] border-blue-500/30' : 'bg-[#0b1222]/40 border-white/5'"
      >
        <button
          @click="toggleFaq(i)"
          class="w-full px-8 py-6 flex items-center justify-between text-left group"
        >
          <span class="text-lg font-bold text-white pr-8">{{ faq.p }}</span>
          <div
            class="w-8 h-8 rounded-full border border-white/10 flex items-center justify-center transition-all duration-300 group-hover:border-blue-500/50"
            :class="activeFaq === i ? 'rotate-45 border-blue-500 bg-blue-500/10' : ''"
          >
            <span class="icon-[ic--baseline-add] text-white transition-colors" :class="activeFaq === i ? 'text-blue-400' : ''"></span>
          </div>
        </button>
        <div v-show="activeFaq === i" class="px-8 pb-8 animate-in fade-in slide-in-from-top-2 duration-300">
          <p class="text-slate-400 leading-relaxed">{{ faq.r }}</p>
        </div>
      </div>
    </div>

    <div class="bg-blue-600/5 border border-blue-500/20 p-10 rounded-3xl text-center group transition-all hover:bg-blue-600/10">
      <h4 class="text-xl font-bold text-white mb-6">¿Tienes otra pregunta? Escríbenos directo por WhatsApp.</h4>
      <a href="https://wa.me/18296932458" target="_blank" class="inline-flex items-center gap-3 px-8 py-4 bg-blue-600 hover:bg-blue-500 text-white rounded-xl font-black uppercase tracking-widest text-xs transition-all shadow-[0_10px_20px_rgba(37,99,235,0.2)]">
        <span class="icon-[ic--baseline-whatsapp] text-xl"></span>
        Hablar con Soporte
      </a>
    </div>
  </section>
</template>
