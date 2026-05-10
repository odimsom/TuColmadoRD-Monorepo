<template>
  <div id="beneficios" class="max-w-7xl mx-auto mb-48 scroll-mt-32">
    <div class="grid grid-cols-1 md:grid-cols-3 gap-10">
      <div class="group relative p-12 rounded-[2rem] bg-white/[0.03] border border-white/10 backdrop-blur-xl transition-all duration-500 hover:-translate-y-4 hover:shadow-[0_30px_60px_rgba(0,0,0,0.5)] hover:border-blue-500/30 overflow-hidden">
        <div class="w-16 h-16 bg-blue-600 rounded-2xl flex items-center justify-center mb-10 shadow-[0_10px_20px_rgba(37,99,235,0.3)] group-hover:scale-110 transition-transform duration-500">
          <span class="icon-[ic--baseline-inventory] text-3xl text-white"></span>
        </div>
        <h3 class="text-3xl font-bebas text-white mb-4 tracking-wider uppercase">Inventario Real</h3>
        <p class="text-slate-400 text-lg leading-relaxed">Sabe exactamente cuántas fundas de pan o botellones de agua te quedan sin tener que salir al mostrador.</p>
        <div class="absolute bottom-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-blue-500/50 to-transparent scale-x-0 group-hover:scale-x-100 transition-transform duration-500"></div>
      </div>

      <div class="group relative p-12 rounded-[2rem] bg-white/[0.03] border border-white/10 backdrop-blur-xl transition-all duration-500 hover:-translate-y-4 hover:shadow-[0_30px_60px_rgba(0,0,0,0.5)] hover:border-red-500/30 overflow-hidden">
        <div class="w-16 h-16 bg-red-600 rounded-2xl flex items-center justify-center mb-10 shadow-[0_10px_20px_rgba(153,27,27,0.3)] group-hover:scale-110 transition-transform duration-500">
          <span class="icon-[ic--baseline-menu-book] text-3xl text-white"></span>
        </div>
        <h3 class="text-3xl font-bebas text-white mb-4 tracking-wider uppercase">Adiós a la Libreta</h3>
        <p class="text-slate-400 text-lg leading-relaxed">Cuentas claras, amistades largas. Registra los fiados de tus clientes y elimina los errores de suma para siempre.</p>
        <div class="absolute bottom-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-red-500/50 to-transparent scale-x-0 group-hover:scale-x-100 transition-transform duration-500"></div>
      </div>

      <div class="group relative p-12 rounded-[2rem] bg-white/[0.03] border border-white/10 backdrop-blur-xl transition-all duration-500 hover:-translate-y-4 hover:shadow-[0_30px_60px_rgba(0,0,0,0.5)] hover:border-blue-500/30 overflow-hidden">
        <div class="w-16 h-16 bg-blue-600 rounded-2xl flex items-center justify-center mb-10 shadow-[0_10px_20px_rgba(37,99,235,0.3)] group-hover:scale-110 transition-transform duration-500">
          <span class="icon-[ic--baseline-point-of-sale] text-3xl text-white"></span>
        </div>
        <h3 class="text-3xl font-bebas text-white mb-4 tracking-wider uppercase">Cuadre de Caja</h3>
        <p class="text-slate-400 text-lg leading-relaxed">Cierra tu turno al centavo. El sistema suma ventas y resta gastos para decirte cuánto dinero exacto debe haber en caja.</p>
        <div class="absolute bottom-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-blue-500/50 to-transparent scale-x-0 group-hover:scale-x-100 transition-transform duration-500"></div>
      </div>
    </div>
  </div>
</template>
