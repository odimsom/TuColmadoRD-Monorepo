<script setup lang="ts">
defineOptions({ name: 'ProductShowcaseSection' })
</script>

<template>
  <section id="producto" class="py-32 bg-base-200">
    <div class="container mx-auto px-6">
      <div class="text-center mb-16 reveal">
        <span class="inline-block px-3.5 py-1.5 bg-primary/10 text-primary text-[11px] font-black uppercase tracking-[0.20em] mb-6">
          Así se ve por dentro
        </span>
        <h2 class="text-5xl md:text-7xl font-black text-base-content tracking-tighter uppercase leading-[0.85] italic mt-6">
          Una herramienta,<br />
          <span class="text-primary">todo tu colmado</span>
        </h2>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-7xl mx-auto">

        <!-- POS mock -->
        <div class="flex flex-col gap-5 reveal" style="transition-delay: 0ms">
          <div class="bg-white shadow-xl overflow-hidden h-70">
            <div class="flex h-full">
              <!-- Products grid -->
              <div class="flex-1 p-3.5 bg-base-200">
                <div class="flex gap-2 mb-2.5">
                  <span v-for="(cat, i) in ['Granos', 'Bebidas', 'Lácteos', 'Limpieza']" :key="cat"
                    :class="['px-2.5 py-1 text-[9px] font-bold uppercase tracking-[0.10em] border',
                    i === 0 ? 'bg-primary text-white border-primary' : 'bg-white text-base-content/60 border-base-300']">
                    {{ cat }}
                  </span>
                </div>
                <div class="grid grid-cols-3 gap-2">
                  <div v-for="[name, price] in [['Arroz Rico 5lb', '195'], ['Aceite Crisol', '320'], ['Leche Rica 1L', '95'], ['Pan Cibao', '60'], ['Pollo libra', '85'], ['Habichuelas', '110']]"
                    :key="name"
                    class="bg-white border border-base-300 p-2 text-center cursor-pointer hover:border-primary/30 hover:bg-primary/5 transition-colors">
                    <div class="w-8 h-8 bg-base-300 mx-auto mb-1.5"></div>
                    <div class="text-[10px] font-semibold text-base-content leading-tight min-h-[22px]">{{ name }}</div>
                    <div class="text-[11px] font-black text-primary mt-1">RD$ {{ price }}</div>
                  </div>
                </div>
              </div>
              <!-- Cart -->
              <div class="w-36 p-3.5 bg-white border-l border-base-300 flex flex-col">
                <div class="text-[9px] font-black uppercase tracking-[0.20em] text-base-content/40 mb-2.5">Carrito · 4</div>
                <div v-for="[n, p] in [['Arroz Rico', 195], ['Aceite Crisol', 320], ['Leche Rica', 95], ['Pan Cibao', 60]]" :key="n"
                  class="flex justify-between text-[10px] leading-[1.5] text-base-content/60">
                  <span>{{ n }}</span><span class="text-base-content">{{ p }}</span>
                </div>
                <div class="mt-auto border-t border-base-300 pt-2.5">
                  <div class="flex justify-between text-[14px] font-black">
                    <span class="text-base-content">Total</span>
                    <span class="text-primary">RD$ 670</span>
                  </div>
                  <button class="mt-2.5 w-full bg-primary text-white border-0 py-2.5 text-[10px] font-black uppercase tracking-[0.18em] cursor-pointer">
                    Cobrar
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div>
            <h4 class="text-sm font-black uppercase tracking-[0.10em] text-base-content mb-2">Punto de venta</h4>
            <p class="text-sm text-base-content/60 leading-relaxed">Vende rápido con búsqueda, carrito, e-CF y cobro en efectivo, tarjeta o fiado.</p>
          </div>
        </div>

        <!-- Dashboard mock -->
        <div class="flex flex-col gap-5 reveal" style="transition-delay: 150ms">
          <div class="bg-white shadow-xl p-4 h-70 flex flex-col gap-3">
            <div class="grid grid-cols-3 gap-2">
              <div v-for="[label, value, delta, color] in [['Ventas hoy', 'RD$ 12,450', '+18%', 'text-primary'], ['Productos', '847', null, 'text-base-content'], ['Fiados', 'RD$ 4,180', null, 'text-secondary']]"
                :key="label"
                class="border border-base-300 px-2.5 py-2">
                <div class="text-[8px] font-black uppercase tracking-[0.18em] text-base-content/40">{{ label }}</div>
                <div :class="['text-base font-black mt-1', color]">{{ value }}</div>
                <div v-if="delta" class="text-[9px] font-bold text-success mt-0.5">↑ {{ delta }} vs ayer</div>
              </div>
            </div>
            <div class="flex-1 border border-base-300 p-3 flex flex-col">
              <div class="text-[9px] font-black uppercase tracking-[0.18em] text-base-content/40 mb-2">Ventas · 7 días</div>
              <svg viewBox="0 0 280 80" preserveAspectRatio="none" class="flex-1 w-full">
                <polyline points="0,60 40,45 80,52 120,30 160,38 200,20 240,28 280,12"
                  fill="none" stroke="var(--color-primary)" stroke-width="2" />
                <polyline points="0,60 40,45 80,52 120,30 160,38 200,20 240,28 280,12 280,80 0,80"
                  fill="rgb(0 71 171 / 0.10)" stroke="none" />
              </svg>
              <div class="flex justify-between text-[8px] font-mono text-base-content/40 mt-1">
                <span>L</span><span>M</span><span>M</span><span>J</span><span>V</span><span>S</span><span>D</span>
              </div>
            </div>
          </div>
          <div>
            <h4 class="text-sm font-black uppercase tracking-[0.10em] text-base-content mb-2">Dashboard</h4>
            <p class="text-sm text-base-content/60 leading-relaxed">Ventas del día, productos top, fiados pendientes y tendencias en una sola vista.</p>
          </div>
        </div>

        <!-- Fiados mock -->
        <div class="flex flex-col gap-5 reveal" style="transition-delay: 300ms">
          <div class="bg-white shadow-xl h-70 flex flex-col">
            <div class="flex justify-between items-center px-3.5 py-3 border-b border-base-300">
              <span class="text-[11px] font-black text-base-content">Fiados por cobrar</span>
              <span class="text-[11px] font-black text-secondary">RD$ 4,180</span>
            </div>
            <div class="flex-1 overflow-hidden">
              <div v-for="row in [
                { n: 'Doña Rosa Méndez', amt: '1,840', aging: 21, status: 'overdue' },
                { n: 'Luis · El Compai', amt: '720', aging: 9, status: 'soon' },
                { n: 'Maritza Pérez', amt: '1,150', aging: 14, status: 'soon' },
                { n: 'Junior del Block', amt: '380', aging: 4, status: 'fresh' },
                { n: 'Sra. Altagracia', amt: '90', aging: 2, status: 'fresh' },
              ]" :key="row.n"
                class="flex items-center gap-2.5 px-3.5 py-2 border-b border-base-200">
                <div class="w-6 h-6 bg-base-300 text-base-content/60 grid place-items-center text-[10px] font-black rounded-full shrink-0">
                  {{ (row.n as string).split(' ').slice(0, 2).map((w: string) => w[0]).join('') }}
                </div>
                <span class="flex-1 text-[11px] font-medium text-base-content truncate" v-html="row.n"></span>
                <span :class="['px-1.5 py-0.5 text-[9px] font-bold rounded-full',
                  row.status === 'overdue' ? 'bg-secondary/10 text-secondary' :
                  row.status === 'soon'    ? 'bg-warning/20 text-warning' :
                  'bg-success/20 text-success']">{{ row.aging }}d</span>
                <span class="text-[11px] font-black text-base-content min-w-[48px] text-right">{{ row.amt }}</span>
              </div>
            </div>
          </div>
          <div>
            <h4 class="text-sm font-black uppercase tracking-[0.10em] text-base-content mb-2">Fiados por cobrar</h4>
            <p class="text-sm text-base-content/60 leading-relaxed">Lista priorizada por antigüedad. Nunca más olvidas a quién le debes cobrar.</p>
          </div>
        </div>

      </div>
    </div>
  </section>
</template>
