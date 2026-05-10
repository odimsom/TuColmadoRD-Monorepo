<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useWebAdmin } from '../composables/useWebAdmin'

const { homeUrl, registerUrl } = useWebAdmin()
const isScrolled = ref(false)
const mobileMenuOpen = ref(false)

onMounted(() => {
  window.addEventListener('scroll', () => {
    isScrolled.value = window.scrollY > 20
  })
})

const closeMenu = () => { mobileMenuOpen.value = false }
</script>

<template>
  <header
    :class="[
      'fixed w-full top-0 z-50 transition-all duration-300 px-6 md:px-8',
      isScrolled ? 'bg-[#0f172a]/95 backdrop-blur-xl border-b border-white/5 py-3' : 'bg-transparent py-5'
    ]"
  >
    <div class="container mx-auto flex justify-between items-center">
      <!-- Logo -->
      <a href="#inicio" class="flex items-center gap-3 group cursor-pointer transition-all hover:opacity-80" @click="closeMenu">
        <div class="relative">
          <img src="/assets/logo.svg" alt="TuColmadoRD logo" class="w-9 h-9 transition-transform duration-500 group-hover:rotate-12" />
          <div class="absolute inset-0 bg-blue-500/20 blur-lg rounded-full scale-0 group-hover:scale-150 transition-transform duration-500"></div>
        </div>
        <div class="flex flex-col justify-center leading-none">
          <span class="text-xl font-bold text-white tracking-tight leading-none">TuColmado</span>
          <span class="text-[0.65rem] font-bold text-blue-500 uppercase tracking-[0.4em] mt-0.5">RD</span>
        </div>
      </a>

      <!-- Nav desktop -->
      <nav class="hidden md:flex space-x-10">
        <a href="#inicio" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Inicio</a>
        <a href="#beneficios" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Beneficios</a>
        <a href="#como-funciona" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Cómo Funciona</a>
        <a href="#precios" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Precios</a>
      </nav>

      <!-- CTAs desktop -->
      <div class="hidden md:flex items-center gap-5">
        <a :href="homeUrl" class="text-slate-400 hover:text-white transition-all text-xs font-black uppercase tracking-widest">
          Iniciar Sesión
        </a>
        <a :href="registerUrl" class="px-5 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-white font-black transition-all shadow-[0_0_20px_rgba(37,99,235,0.3)] text-xs uppercase tracking-widest border border-blue-400/20">
          Postular →
        </a>
      </div>

      <!-- Hamburger button -->
      <button
        @click="mobileMenuOpen = !mobileMenuOpen"
        class="md:hidden flex flex-col gap-1.5 p-2 rounded-lg hover:bg-white/5 transition-colors"
        aria-label="Abrir menú"
      >
        <span :class="['block w-6 h-0.5 bg-white transition-all duration-300', mobileMenuOpen ? 'rotate-45 translate-y-2' : '']"></span>
        <span :class="['block w-6 h-0.5 bg-white transition-all duration-300', mobileMenuOpen ? 'opacity-0' : '']"></span>
        <span :class="['block w-6 h-0.5 bg-white transition-all duration-300', mobileMenuOpen ? '-rotate-45 -translate-y-2' : '']"></span>
      </button>
    </div>

    <!-- Mobile menu -->
    <div
      v-show="mobileMenuOpen"
      class="md:hidden mt-4 pb-6 border-t border-white/5 animate-in fade-in slide-in-from-top-2 duration-200"
    >
      <nav class="flex flex-col gap-1 pt-4">
        <a href="#inicio" @click="closeMenu" class="px-4 py-3 text-slate-300 hover:text-white hover:bg-white/5 rounded-lg transition-all font-medium text-sm uppercase tracking-widest">Inicio</a>
        <a href="#beneficios" @click="closeMenu" class="px-4 py-3 text-slate-300 hover:text-white hover:bg-white/5 rounded-lg transition-all font-medium text-sm uppercase tracking-widest">Beneficios</a>
        <a href="#como-funciona" @click="closeMenu" class="px-4 py-3 text-slate-300 hover:text-white hover:bg-white/5 rounded-lg transition-all font-medium text-sm uppercase tracking-widest">Cómo Funciona</a>
        <a href="#precios" @click="closeMenu" class="px-4 py-3 text-slate-300 hover:text-white hover:bg-white/5 rounded-lg transition-all font-medium text-sm uppercase tracking-widest">Precios</a>
      </nav>
      <div class="flex flex-col gap-3 mt-4 px-4">
        <a :href="homeUrl" class="py-3 text-center text-slate-400 hover:text-white transition-all text-xs font-black uppercase tracking-widest border border-white/10 rounded-lg">
          Iniciar Sesión
        </a>
        <a :href="registerUrl" class="py-3 text-center rounded-lg bg-blue-600 hover:bg-blue-500 text-white font-black transition-all text-xs uppercase tracking-widest shadow-[0_0_20px_rgba(37,99,235,0.3)]">
          Postular →
        </a>
      </div>
    </div>
  </header>
</template>
