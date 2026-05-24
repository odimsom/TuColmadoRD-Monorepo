<script setup lang="ts">
import { Icon } from '@iconify/vue'

defineOptions({ name: 'AntesDespuesSection' })

const benefits = [
  { icon: 'lucide:eye', title: 'Ves todo de un vistazo', body: 'Quién debe, cuánto y desde cuándo. Sin buscar páginas en un cuaderno.' },
  { icon: 'lucide:bell', title: 'Te recuerda automático', body: 'Cuando un fiado pasa de 14 días te avisa para que cobres a tiempo.' },
  { icon: 'lucide:shield-check', title: 'No se pierde nada', body: 'Los registros se respaldan en la nube. Si pierdes el cuaderno, no pierdes el dinero.' },
]
</script>

<template>
  <section class="py-32 bg-base-100">
    <div class="container mx-auto px-6">
      <div class="text-center mb-16 reveal">
        <span class="inline-block px-3.5 py-1.5 bg-secondary/10 text-secondary text-[11px] font-black uppercase tracking-[0.20em] mb-6">
          El cambio real
        </span>
        <h2 class="text-5xl md:text-7xl font-black text-base-content tracking-tighter uppercase leading-[0.85] italic mt-6">
          Del cuaderno<br />
          <span class="text-secondary">a la pantalla</span>
        </h2>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-5xl mx-auto">

        <!-- Antes — cuaderno manuscrito -->
        <div class="relative min-h-[420px] shadow-xl p-9 pb-7 reveal"
          style="background-color: #f5ecd9; background-image: repeating-linear-gradient(0deg, transparent 0, transparent 27px, rgba(99,49,49,0.18) 27px, rgba(99,49,49,0.18) 28px);">
          <div class="absolute top-3.5 right-4 px-3 py-1 bg-neutral text-white text-[10px] font-black uppercase tracking-[0.20em]">
            Antes
          </div>
          <div class="handwritten text-[26px] text-[#3a2a1a] leading-[28px]" style="transform: rotate(-0.4deg)">
            <div class="border-b-2 border-[#3a2a1a] inline-block mb-2 pb-0.5">Fiados · Octubre</div>
            <div>Doña Rosa ... 1,840</div>
            <div>Luis (el compai) · 720</div>
            <div class="line-through text-[#777]">Maritza ... 1,150 ?</div>
            <div>Junior del block ... 380</div>
            <div class="mt-1 text-[#a04040]">Sra. Alta... cuánto?? 90 o 190</div>
            <div>El moreno de la esquina ... 240</div>
            <div class="line-through text-[#777]">Manuel ... 600</div>
            <div class="mt-1.5 italic text-[#7a5530] text-[22px]">* hay un fiado que no sé de quién es *</div>
          </div>
        </div>

        <!-- Después — mini Fiados UI -->
        <div class="min-h-[420px] flex reveal" style="transition-delay: 150ms">
          <div class="flex-1 bg-white shadow-xl flex flex-col">
            <div class="flex justify-between items-center px-3.5 py-3 border-b border-base-300">
              <span class="text-[11px] font-black text-base-content">Fiados por cobrar</span>
              <span class="text-[11px] font-black text-secondary">RD$ 4,180</span>
            </div>
            <div class="flex-1">
              <div v-for="row in [
                { n: 'Doña Rosa Méndez', amt: '1,840', aging: 21, status: 'overdue' },
                { n: 'Luis · El Compai', amt: '720', aging: 9, status: 'soon' },
                { n: 'Maritza Pérez', amt: '1,150', aging: 14, status: 'soon' },
                { n: 'Junior del Block', amt: '380', aging: 4, status: 'fresh' },
                { n: 'Sra. Altagracia', amt: '90', aging: 2, status: 'fresh' },
                { n: 'El moreno de la esquina', amt: '240', aging: 11, status: 'soon' },
              ]" :key="row.n"
                class="flex items-center gap-2.5 px-3.5 py-2.5 border-b border-base-200">
                <div class="w-7 h-7 bg-base-300 text-base-content/60 grid place-items-center text-[10px] font-black rounded-full shrink-0">
                  {{ row.n.split(' ').slice(0, 2).map((w: string) => w[0]).join('') }}
                </div>
                <span class="flex-1 text-[12px] font-medium text-base-content truncate">{{ row.n }}</span>
                <span :class="['px-2 py-0.5 text-[9px] font-bold rounded-full',
                  row.status === 'overdue' ? 'bg-secondary/10 text-secondary' :
                  row.status === 'soon'    ? 'bg-warning/20 text-warning' :
                  'bg-success/20 text-success']">{{ row.aging }}d</span>
                <span class="text-[12px] font-black text-base-content min-w-[52px] text-right">{{ row.amt }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Benefits row -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-5xl mx-auto mt-12">
        <div v-for="b in benefits" :key="b.title" class="flex gap-3.5 reveal">
          <div class="w-10 h-10 bg-primary text-white flex items-center justify-center shrink-0">
            <Icon :icon="b.icon" class="w-5 h-5" />
          </div>
          <div>
            <h4 class="text-xs font-black uppercase tracking-[0.10em] text-base-content mb-1.5">{{ b.title }}</h4>
            <p class="text-[13px] text-base-content/60 leading-relaxed">{{ b.body }}</p>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Caveat:wght@400;700&display=swap");
.handwritten {
  font-family: "Caveat", "Comic Sans MS", cursive;
}
</style>
