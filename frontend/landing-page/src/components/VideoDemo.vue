<template>
  <section class="max-w-4xl mx-auto mb-48 text-center bg-[#080E1A] p-12 md:p-20 rounded-[3rem] border border-white/5 shadow-2xl relative overflow-hidden">
    <div class="absolute inset-0 bg-[radial-gradient(circle_at_50%_50%,rgba(37,99,235,0.05),transparent)]"></div>

    <div class="relative z-10">
      <span class="px-4 py-1.5 rounded-full border border-blue-600/30 bg-blue-600/5 text-blue-400 text-xs font-black uppercase tracking-[0.2em] mb-6 inline-block">MÍRALO EN ACCIÓN</span>
      <h2 class="text-6xl md:text-8xl font-bebas text-white mb-6 uppercase tracking-widest leading-none">Ve el sistema antes de postular</h2>
      <p class="text-xl text-slate-400 mb-12 max-w-2xl mx-auto">
        Un demo real del POS. Sin actores, sin scripts. Así trabaja TuColmadoRD en un colmado real.
      </p>

      <div class="relative aspect-video w-full rounded-2xl border border-blue-900/40 overflow-hidden bg-slate-900 shadow-[0_0_100px_-20px_rgba(0,0,0,0.8)]">
        <video
          class="w-full h-full object-cover"
          controls
          preload="metadata"
          poster="/assets/images/hero-colmado.png"
        >
          <source src="/assets/videos/demo-tucolmadord.webm" type="video/webm" />
          Tu navegador no soporta video HTML5.
        </video>
      </div>

      <div class="mt-8 flex items-center justify-center gap-2 text-blue-400 font-bold text-sm">
        <span class="w-1.5 h-1.5 bg-blue-400 rounded-full"></span>
        Demo real · Sin registro requerido
      </div>
    </div>
  </section>
</template>
