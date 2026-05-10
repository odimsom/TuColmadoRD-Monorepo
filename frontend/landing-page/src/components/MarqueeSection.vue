<template>
  <div class="bg-blue-600 py-4 overflow-hidden border-y border-white/10 relative z-20 md:-rotate-1 -rotate-0 origin-center scale-105">
    <div class="flex animate-marquee whitespace-nowrap" style="animation-duration:60s;">
      <div v-for="i in 10" :key="i" class="flex items-center mx-4">
        <span class="text-white font-bebas text-2xl md:text-3xl tracking-widest uppercase">INVENTARIO · FIADOS · CUADRE DE CAJA · FACTURACIÓN RÁPIDA · </span>
      </div>
    </div>
  </div>
</template>
