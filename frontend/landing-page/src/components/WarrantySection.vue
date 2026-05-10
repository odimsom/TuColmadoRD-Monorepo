<template>
  <div class="max-w-6xl mx-auto mb-48 relative overflow-hidden rounded-[4rem] border border-white/10">
    <div class="absolute inset-0 bg-[#0F172A]"></div>
    <svg class="absolute inset-0 w-full h-full opacity-[0.05]" width="100%" height="100%">
      <defs>
        <pattern id="diagonal-dots" x="0" y="0" width="40" height="40" patternUnits="userSpaceOnUse">
          <line x1="0" y1="40" x2="40" y2="0" stroke="#2563eb" stroke-width="1" />
          <circle cx="2" cy="2" r="1.5" fill="#2563eb" />
        </pattern>
      </defs>
      <rect width="100%" height="100%" fill="url(#diagonal-dots)" />
    </svg>

    <div class="relative bg-blue-900/10 p-24 text-center backdrop-blur-xl border border-blue-500/10 overflow-hidden group">
      <div class="absolute -right-20 -bottom-20 opacity-10 group-hover:rotate-12 transition-transform duration-1000">
        <span class="icon-[ic--baseline-verified-user] text-[20rem]"></span>
      </div>

      <h2 class="text-7xl md:text-9xl font-bebas text-white mb-10 tracking-widest uppercase opacity-90">Garantía de Cuadre</h2>
      <p class="text-2xl md:text-5xl text-slate-300 leading-tight max-w-4xl mx-auto font-light mb-4">
        Con TuColmadoRD no hay "perdedera". Te aseguramos que
      </p>
      <div class="relative inline-block mt-4">
        <span class="text-3xl md:text-6xl font-black text-white uppercase tracking-tighter relative z-10 px-8 py-4">
          cada peso está contado
        </span>
        <div class="absolute inset-0 bg-blue-600/20 blur-2xl z-0 scale-75 group-hover:scale-110 transition-transform duration-1000"></div>
        <span class="absolute bottom-0 left-0 h-[8px] bg-blue-600 z-10 shadow-[0_0_20px_rgba(37,99,235,0.8)] animate-[width-grow_3s_ease-in-out_infinite]"></span>
      </div>
      <p class="text-xl text-slate-500 mt-12 font-medium tracking-widest uppercase">Seguridad total para tu negocio</p>
    </div>
  </div>
</template>
