<script setup lang="ts">
import { useWebAdmin } from '../composables/useWebAdmin'

const { registerUrl } = useWebAdmin()
</script>

<template>
  <section id="inicio" class="relative w-full h-screen sm:h-auto sm:min-h-[55vh] md:h-[90vh] flex items-center overflow-hidden border-b border-white/5 scroll-mt-20">
    <div class="absolute inset-0 bg-cover bg-center" style="background-image: url('/assets/images/hero-colmado.png');" aria-hidden="true"></div>
    <div class="absolute inset-0 bg-gradient-to-r from-[#0f172a] via-[#0f172a]/95 to-[#0f172a]/40" aria-hidden="true"></div>

    <div class="container mx-auto px-6 md:px-12 relative z-10">
      <div class="max-w-4xl animate-in fade-in slide-in-from-left-12 duration-1000">
        <div class="inline-flex items-center gap-3 px-4 py-2 rounded-full bg-blue-500/10 border border-blue-500/20 text-blue-400 text-xs font-black uppercase tracking-[0.3em] mb-16 backdrop-blur-md">
          <span class="w-2 h-2 bg-blue-500 rounded-full animate-pulse"></span>
          Programa Piloto - Usuarios Pioneros
        </div>
        <h1 class="max-w-3xl font-bebas text-white leading-[0.95] mb-8 md:mb-12 tracking-tight uppercase drop-shadow-2xl" style="font-size:clamp(1.875rem,5.5vw,4.5rem);">
          Tu <span class="text-blue-500 drop-shadow-[0_0_30px_rgba(37,99,235,0.5)]">Colmado</span><br/>Sin Costo. <br/>Un Año Completo.
        </h1>
        <p class="text-xl md:text-2xl text-slate-400 max-w-2xl font-medium leading-relaxed mb-6 opacity-90">
          Estamos en etapa de lanzamiento y buscamos a nuestros primeros 20 clientes pioneros para revolucionar el barrio.
          Sin promesas de corporativo. Con soporte real por WhatsApp.
        </p>
        <p class="text-lg md:text-xl text-blue-400 max-w-2xl font-semibold mb-8">
          ✓ 1 año de acceso completo (gratis)
          <br/>
          ✓ Soporte humano dominicano
          <br/>
          ✓ Tu feedback nos define
        </p>
        <div class="flex flex-col sm:flex-row gap-4 mb-12">
          <a :href="registerUrl" class="group relative px-6 sm:px-8 py-4 sm:py-5 bg-blue-600 hover:bg-blue-500 text-white font-black rounded-lg transition-all shadow-[0_0_40px_rgba(37,99,235,0.4)] text-sm sm:text-base uppercase tracking-widest overflow-hidden flex items-center justify-center sm:justify-start gap-2 flex-wrap">
            <span class="relative z-10">Postular mi colmado</span>
            <span class="relative z-10 text-blue-300 hidden sm:inline">→</span>
            <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-1000"></div>
          </a>
          <a href="#como-funciona" class="px-6 sm:px-8 py-4 sm:py-5 bg-white/5 hover:bg-white/10 border border-white/10 text-white font-black rounded-lg transition-all text-sm sm:text-base uppercase tracking-widest text-center">
            Ver Demo
          </a>
        </div>
        <div class="flex items-center gap-4 flex-wrap">
          <div class="flex flex-col gap-3 w-full">
            <div class="flex items-start sm:items-center gap-3 p-4 rounded-lg bg-blue-500/10 border border-blue-500/20 flex-wrap">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="font-black text-blue-400 text-base sm:text-lg whitespace-nowrap">Licencias Gratuitas</span>
                <span class="text-xs sm:text-sm text-slate-400">Cupos limitados para los primeros 20 clientes</span>
              </div>
            </div>
            <div class="w-full bg-slate-700 rounded-full h-2.5 overflow-hidden">
              <div class="bg-blue-500 h-full rounded-full" style="width: 0%"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>
