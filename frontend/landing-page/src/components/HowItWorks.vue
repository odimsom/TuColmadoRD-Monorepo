<script setup lang="ts">
const steps = [
  { n: '01', i: 'icon-[ic--baseline-how-to-reg]', t: 'Regístrate en 2 minutos', d: 'Crea tu cuenta con tu nombre y el nombre de tu colmado. Sin papeleos, sin tarjeta de crédito para empezar.' },
  { n: '02', i: 'icon-[ic--baseline-inventory-2]', t: 'Carga tu inventario', d: 'Agrega tus productos con precio y cantidad. Puedes importar desde Excel. Sencillo como anotar en la libreta.' },
  { n: '03', i: 'icon-[ic--baseline-point-of-sale]', t: 'Empieza a vender', d: 'Abre el POS, selecciona productos y cobra. El sistema registra ventas, fiados y cuadre automáticamente.' },
  { n: '04', i: 'icon-[ic--baseline-analytics]', t: 'Ve tus reportes', d: 'Al final del turno el sistema te dice exactamente cuánto dinero debe haber en la gaveta. Sin sorpresas.' }
]
</script>

<template>
  <section id="como-funciona" class="max-w-7xl mx-auto mb-48 pt-20 scroll-mt-32 relative">
    <div class="absolute left-1/2 top-40 w-px h-[60%] bg-gradient-to-b from-blue-500/40 via-blue-500/10 to-transparent hidden lg:block"></div>

    <div class="text-center mb-20 relative z-10">
      <span class="px-4 py-1.5 rounded-full border border-blue-500/30 bg-blue-500/5 text-blue-400 text-xs font-black uppercase tracking-[0.2em] mb-6 inline-block">SIN COMPLICACIONES</span>
      <h2 class="text-6xl md:text-8xl font-bebas text-white tracking-widest uppercase">Cómo funciona</h2>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 relative z-10">
      <div v-for="(step, i) in steps" :key="i" class="p-8 rounded-2xl bg-[#0F1F3D] border border-blue-900/20 transition-all duration-300 hover:-translate-y-1 hover:border-blue-500/50 group">
        <div class="font-bebas text-5xl text-[#1E3A8A] mb-4 transition-colors group-hover:text-blue-500/30">{{ step.n }}</div>
        <div class="w-12 h-12 rounded-xl bg-blue-500/5 flex items-center justify-center mb-6 group-hover:bg-blue-500/10 transition-colors">
          <span :class="[step.i, 'text-3xl text-blue-500/50 group-hover:text-blue-400 transition-all']"></span>
        </div>
        <h4 class="text-xl font-bold text-white mb-3">{{ step.t }}</h4>
        <p class="text-slate-400 text-sm leading-relaxed">{{ step.d }}</p>
      </div>
    </div>
  </section>
</template>
