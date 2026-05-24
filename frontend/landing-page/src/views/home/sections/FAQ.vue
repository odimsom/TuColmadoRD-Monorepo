<script setup lang="ts">
import { ref } from 'vue'
import { Icon } from '@iconify/vue'

defineOptions({ name: 'FAQSection' })

const items = [
  {
    q: '¿Y si se va la luz o no hay internet?',
    a: 'Sigues vendiendo. La app guarda las ventas en tu celular y las sincroniza apenas vuelve la conexión. Si usas el programa de escritorio (.exe), funciona offline completo y se actualiza solo cuando el internet regresa.',
    defaultOpen: true,
  },
  {
    q: '¿Mis empleados sabrán usarlo?',
    a: 'Sí. La pantalla de venta está pensada para colmaderos, no para programadores. Cualquier cajero entiende cómo cobrar y dar vuelto en menos de 5 minutos. Te entrenamos por WhatsApp si lo necesitas.',
    defaultOpen: false,
  },
  {
    q: '¿Qué pasa con mis datos al final del año gratis?',
    a: 'Son tuyos. Si decides irte, exportas todo en Excel y te lo llevas. No hay lock-in. Si decides quedarte, los colmados del programa pionero tienen precios preferenciales para siempre.',
    defaultOpen: false,
  },
  {
    q: '¿Necesito comprar caja registradora nueva o equipo especial?',
    a: 'No. Corre en cualquier celular Android, iPhone, tableta o computadora con navegador. Si quieres impresora de recibos térmica te recomendamos modelos baratos, pero es opcional.',
    defaultOpen: false,
  },
  {
    q: '¿Sirve para mi colmado pequeño?',
    a: 'Justamente para eso lo hicimos. El plan Básico empieza en RD$1,200/mes y el programa pionero da el primer año gratis. Tres categorías y 50 productos es suficiente para arrancar.',
    defaultOpen: false,
  },
  {
    q: '¿Puedo emitir e-CF de DGII?',
    a: 'Sí. TuColmadoRD emite Comprobantes Fiscales Electrónicos validados por DGII desde el día uno. Si tu cliente te pide RNC, le entregas el e-CF en el mismo recibo.',
    defaultOpen: false,
  },
  {
    q: '¿Cómo cargo mi inventario actual?',
    a: 'Te ayudamos. Llegas con tu cuaderno, lista en Excel o foto de las góndolas y nuestro equipo te lo carga, gratis, en una llamada por WhatsApp. También puedes subir un CSV si ya lo tienes digital.',
    defaultOpen: false,
  },
]

const openIndex = ref<number | null>(0)

function toggle(i: number) {
  openIndex.value = openIndex.value === i ? null : i
}
</script>

<template>
  <section id="faq" class="py-32 bg-base-200">
    <div class="container mx-auto px-6 max-w-4xl">
      <div class="text-center mb-14 reveal">
        <span class="inline-block px-3.5 py-1.5 bg-base-300 text-base-content/60 text-[11px] font-black uppercase tracking-[0.20em] mb-6">
          Preguntas frecuentes
        </span>
        <h2 class="text-5xl md:text-6xl font-black text-base-content tracking-tighter leading-[0.9] italic mt-6">
          Todo lo que te<br />
          <span class="text-primary">preguntas antes</span>
        </h2>
      </div>

      <div class="reveal">
        <div v-for="(item, i) in items" :key="i" class="border-b border-base-300">
          <button
            type="button"
            class="w-full flex justify-between items-center gap-4 py-6 text-left cursor-pointer bg-transparent border-0 outline-none focus-visible:outline-2 focus-visible:outline-primary focus-visible:outline-offset-2"
            :aria-expanded="openIndex === i"
            @click="toggle(i)"
          >
            <span class="text-lg font-black text-base-content">{{ item.q }}</span>
            <Icon
              :icon="openIndex === i ? 'lucide:minus' : 'lucide:plus'"
              class="w-5 h-5 text-primary shrink-0 transition-transform duration-150"
            />
          </button>
          <Transition name="faq-expand">
            <div v-if="openIndex === i" class="pb-6 text-[15px] leading-[1.65] text-base-content/60 max-w-3xl">
              {{ item.a }}
            </div>
          </Transition>
        </div>
      </div>

      <p class="text-center mt-12 text-sm text-base-content/60 leading-relaxed">
        ¿Te queda alguna duda? Escríbenos por WhatsApp y la respondemos en minutos.
      </p>
    </div>
  </section>
</template>

<style scoped>
.faq-expand-enter-active,
.faq-expand-leave-active {
  transition: opacity 150ms ease, transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
  transform-origin: top;
}
.faq-expand-enter-from,
.faq-expand-leave-to {
  opacity: 0;
  transform: scaleY(0.97);
}
</style>
