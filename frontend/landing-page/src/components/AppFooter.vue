<template>
  <footer class="bg-[#0b1222] border-t border-white/5 pt-16 pb-8 relative z-10">
    <div class="max-w-7xl mx-auto px-8">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-12 mb-12 text-left">
        <div class="col-span-1 md:col-span-2">
          <div class="flex items-center gap-3 mb-6">
            <img src="/assets/logo.svg" class="w-7 h-7" alt="TuColmadoRD logo" />
            <span class="text-xl font-bold text-white tracking-tight">TuColmado <span class="text-blue-500">RD</span></span>
          </div>
          <p class="text-slate-400 text-sm leading-relaxed max-w-sm">
            La plataforma definitiva para la digitalización de colmados en República Dominicana. Control de fiados, inventario y ventas en la palma de tu mano.
          </p>
          <div class="flex gap-4 mt-8">
            <a href="#" class="w-10 h-10 rounded-xl bg-white/5 border border-white/5 flex items-center justify-center hover:bg-blue-600 hover:border-blue-500 hover:text-white transition-all text-slate-400">
              <span class="icon-[ic--baseline-facebook] text-lg"></span>
            </a>
            <a href="https://wa.me/18296932458" class="w-10 h-10 rounded-xl bg-white/5 border border-white/5 flex items-center justify-center hover:bg-blue-600 hover:border-blue-500 hover:text-white transition-all text-slate-400">
              <span class="icon-[ic--baseline-whatsapp] text-lg"></span>
            </a>
            <a href="#" class="w-10 h-10 rounded-xl bg-white/5 border border-white/5 flex items-center justify-center hover:bg-blue-600 hover:border-blue-500 hover:text-white transition-all text-slate-400">
              <span class="icon-[ic--baseline-email] text-lg"></span>
            </a>
          </div>
        </div>

        <div>
          <h4 class="text-white font-semibold mb-6 text-sm uppercase tracking-widest">Producto</h4>
          <ul class="space-y-4 text-slate-400 text-sm">
            <li><a href="#beneficios" class="hover:text-blue-500 transition-colors">Funcionalidades</a></li>
            <li><a href="#precios" class="hover:text-blue-500 transition-colors">Precios</a></li>
            <li><a href="#como-funciona" class="hover:text-blue-500 transition-colors">Cómo Funciona</a></li>
          </ul>
        </div>

        <div>
          <h4 class="text-white font-semibold mb-6 text-sm uppercase tracking-widest">Soporte</h4>
          <ul class="space-y-4 text-slate-400 text-sm">
            <li><a href="#faq" class="hover:text-blue-500 transition-colors">Preguntas Frecuentes</a></li>
            <li><a href="https://wa.me/18296932458" class="hover:text-blue-500 transition-colors">WhatsApp 24/7</a></li>
            <li><a href="#" class="hover:text-blue-500 transition-colors">Privacidad</a></li>
          </ul>
        </div>
      </div>

      <div class="pt-8 border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-4">
        <p class="text-slate-500 text-xs text-center">
          © 2026 TuColmado RD. Todos los derechos reservados.
        </p>
        <div class="flex items-center gap-2">
          <span class="text-slate-600 text-xs italic">Diseñado con ❤️ por</span>
          <a href="https://synsetsolutions.com" target="_blank" rel="noopener noreferrer"
             class="text-slate-400 text-xs font-bold hover:text-blue-500 transition-colors">
            Synset Solutions
          </a>
        </div>
      </div>
    </div>
  </footer>
</template>
