<script setup lang="ts">
import AppLogo from '@/components/ui/AppLogo.vue'
</script>

<template>
  <footer class="bg-neutral text-neutral-content pt-20 pb-10 border-t border-neutral-content/5">
    <div class="container mx-auto px-6">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-16 mb-20">
        <div class="col-span-1 md:col-span-2">
          <AppLogo class="mb-8 brightness-0 invert opacity-80" />
          <p class="text-neutral-content/60 max-w-sm text-lg leading-relaxed">
            La plataforma líder en digitalización de colmados en la República Dominicana.
            Llevando la tradición al siguiente nivel tecnológico.
          </p>
        </div>

        <div>
          <h4 class="text-neutral-content font-black mb-8 uppercase tracking-[0.3em] text-xs">Explorar</h4>
          <ul class="space-y-4 text-neutral-content/50 font-bold uppercase tracking-widest text-xs">
            <li><a href="#" class="hover:text-neutral-content transition-colors">Inicio</a></li>
            <li><a href="#caracteristicas" class="hover:text-neutral-content transition-colors">Características</a></li>
            <li><a href="#precios" class="hover:text-neutral-content transition-colors">Precios</a></li>
            <li><a href="#contacto" class="hover:text-neutral-content transition-colors">Contacto</a></li>
          </ul>
        </div>

        <div>
          <h4 class="text-neutral-content font-black mb-8 uppercase tracking-[0.3em] text-xs">Legal</h4>
          <ul class="space-y-4 text-neutral-content/50 font-bold uppercase tracking-widest text-xs">
            <li><span class="text-neutral-content/30">Términos — próximamente</span></li>
            <li><span class="text-neutral-content/30">Privacidad — próximamente</span></li>
          </ul>
        </div>
      </div>

      <div class="pt-10 border-t border-neutral-content/5 flex flex-col md:flex-row justify-between items-center gap-6 text-xs font-bold uppercase tracking-widest text-neutral-content/30">
        <p>© 2026 TuColmadoRD. Marca Registrada.</p>
        <p>
          Creado por
          <a href="https://synsetsolutions.com" target="_blank" class="text-primary hover:text-neutral-content transition-colors">
            Synset Solutions
          </a>
        </p>
      </div>
    </div>
  </footer>
</template>
