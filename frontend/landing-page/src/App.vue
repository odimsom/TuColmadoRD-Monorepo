<script setup lang="ts">
import { ref, onMounted } from 'vue'

const isScrolled = ref(false)
const showVideo = ref(false)
const activeFaq = ref<number | null>(null)

const toggleFaq = (index: number) => {
  activeFaq.value = activeFaq.value === index ? null : index
}

const scrollToTop = () => {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const webAdminBaseUrl = (import.meta.env.VITE_WEB_ADMIN_URL as string | undefined)?.replace(/\/$/, '') || 'http://localhost:4200'
const webAdminHomeUrl = `${webAdminBaseUrl}/`
const webAdminRegisterUrl = `${webAdminBaseUrl}/auth/register`

onMounted(() => {
  window.addEventListener('scroll', () => {
    isScrolled.value = window.scrollY > 20
  })
})

const faqs = [
  {
    p: "¿Necesito internet para usar el sistema?",
    r: "No. TuColmadoRD funciona sin internet. Puedes registrar ventas, fiados e inventario aunque se vaya la luz o el WIFI. Cuando vuelva la conexión, todo se sincroniza automáticamente."
  },
  {
    p: "¿Cuánto tiempo tarda configurar el sistema?",
    r: "La mayoría de colmaderos están operando el mismo día. El proceso es: crear cuenta → agregar productos → abrir turno → empezar a vender. Si tienes una lista en Excel, lo importamos por ti sin costo."
  },
  {
    p: "¿Puedo tener más de un empleado usando el sistema?",
    r: "Sí. Creas usuarios para cada cajero. El sistema registra quién hizo cada venta. El dueño ve todo desde su celular en tiempo real."
  },
  {
    p: "¿Funciona en tablets o solo en computadora?",
    r: "Funciona en cualquier dispositivo: computadora, tablet Android o iPad. El POS está optimizado para pantallas táctiles. Muchos colmaderos lo usan en una tablet de RD$8,000."
  },
  {
    p: "¿Qué pasa si cancelo? ¿Pierdo mis datos?",
    r: "No. Tienes 30 días para exportar toda tu información (ventas, clientes, inventario) después de cancelar. Tus datos siempre son tuyos."
  },
  {
    p: "¿El sistema maneja los fiados (la libreta)?",
    r: "Sí, es una de las funciones más usadas. Registras fiados por cliente, el sistema calcula el balance y avisa cuando alguien lleva mucho tiempo sin pagar. Adiós a la libreta física y a los 'no sé cuánto me debes'."
  },
  {
    p: "¿Tienen soporte en español dominicano?",
    r: "Claro que sí. Soporte 100% dominicano por WhatsApp, de lunes a sábado. Entendemos cómo funciona un colmado porque somos de aquí."
  }
]
</script>

<template>
  <div class="min-h-screen flex flex-col relative overflow-hidden bg-[#0f172a] font-sans selection:bg-blue-500/30">
    <!-- Ambient Background Elements -->
    <div class="absolute top-[-20%] left-[-10%] w-[500px] h-[500px] bg-blue-900/20 rounded-full blur-[120px] pointer-events-none"></div>
    <div class="absolute bottom-[-10%] right-[-5%] w-[500px] h-[500px] bg-blue-900/10 rounded-full blur-[100px] pointer-events-none"></div>

    <header 
      :class="[
        'fixed w-full top-0 z-50 transition-all duration-300 px-8',
        isScrolled ? 'bg-[#0f172a]/90 backdrop-blur-xl border-b border-white/5 py-3' : 'bg-transparent py-6'
      ]"
    >
      <div class="container mx-auto flex justify-between items-center">
        <div class="flex items-center gap-3 group cursor-pointer" @click="scrollToTop">
          <div class="relative">
            <img src="/assets/logo.svg" alt="TuColmadoRD logo" class="w-9 h-9 transition-transform duration-500 group-hover:rotate-12" />
            <div class="absolute inset-0 bg-blue-500/20 blur-lg rounded-full scale-0 group-hover:scale-150 transition-transform duration-500"></div>
          </div>
          <div class="flex flex-col justify-center leading-none">
            <span class="text-[1.35rem] font-bebas text-white tracking-wider uppercase leading-none">TuColmado<span class="text-blue-500">RD</span></span>
            <span class="text-[0.5rem] font-bold text-slate-600 uppercase tracking-[0.35em] mt-0.5">by Synset Solutions</span>
          </div>
        </div>
        <nav class="hidden md:flex space-x-10">
          <a href="#beneficios" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Beneficios</a>
          <a href="#como-funciona" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Cómo Funciona</a>
          <a href="#precios" class="text-slate-400 hover:text-white transition-colors font-medium text-sm uppercase tracking-widest">Precios</a>
        </nav>
          <div class="flex items-center gap-5">
          <a :href="webAdminHomeUrl" class="text-slate-400 hover:text-white transition-all text-xs font-black uppercase tracking-widest">
            Iniciar Sesión
          </a>
          <a :href="webAdminRegisterUrl" class="px-5 py-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-white font-black transition-all shadow-[0_0_20px_rgba(37,99,235,0.3)] text-xs uppercase tracking-widest border border-blue-400/20">
            Postular →
          </a>
        </div>
      </div>
    </header>

    <main class="flex-grow flex flex-col pt-0">
      <!-- Hero Section: Dominican Street Premium -->
      <section class="relative w-full min-h-[60vh] md:h-[90vh] flex items-center overflow-hidden border-b border-white/5">
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
              <a :href="webAdminRegisterUrl" class="group relative px-6 sm:px-8 py-3 sm:py-4 bg-blue-600 hover:bg-blue-500 text-white font-black rounded-lg transition-all shadow-[0_0_40px_rgba(37,99,235,0.4)] text-xs sm:text-sm uppercase tracking-widest overflow-hidden flex items-center justify-center sm:justify-start gap-2 whitespace-nowrap">
                <span class="relative z-10">Postular mi colmado</span>
                <span class="relative z-10 text-blue-300">→</span>
                <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-1000"></div>
              </a>
              <a href="#como-funciona" class="px-6 sm:px-8 py-3 sm:py-4 bg-white/5 hover:bg-white/10 border border-white/10 text-white font-black rounded-lg transition-all text-xs sm:text-sm uppercase tracking-widest text-center">
                Ver Demo
              </a>
            </div>
            <!-- Social proof inline -->
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

      <!-- Marquee Section -->
      <div class="bg-blue-600 py-4 overflow-hidden border-y border-white/10 relative z-20 md:-rotate-1 -rotate-0 origin-center scale-105">
        <div class="flex animate-marquee whitespace-nowrap" style="animation-duration:60s;">
          <div v-for="i in 10" :key="i" class="flex items-center mx-4">
            <span class="text-white font-bebas text-2xl md:text-3xl tracking-widest uppercase">INVENTARIO · FIADOS · CUADRE DE CAJA · FACTURACIÓN RÁPIDA · </span>
          </div>
        </div>
      </div>

      <div class="container mx-auto px-8 py-32">
        <!-- Feature Cards -->
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

        <!-- SECCIÓN A: CÓMO FUNCIONA -->
        <section id="como-funciona" class="max-w-7xl mx-auto mb-48 pt-20 scroll-mt-32 relative">
          <div class="absolute left-1/2 top-40 w-px h-[60%] bg-gradient-to-b from-blue-500/40 via-blue-500/10 to-transparent hidden lg:block"></div>
          
          <div class="text-center mb-20 relative z-10">
            <span class="px-4 py-1.5 rounded-full border border-blue-500/30 bg-blue-500/5 text-blue-400 text-xs font-black uppercase tracking-[0.2em] mb-6 inline-block">SIN COMPLICACIONES</span>
            <h2 class="text-6xl md:text-8xl font-bebas text-white tracking-widest uppercase">Cómo funciona</h2>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 relative z-10">
            <div v-for="(step, i) in [
              { n: '01', i: 'icon-[ic--baseline-how-to-reg]', t: 'Regístrate en 2 minutos', d: 'Crea tu cuenta con tu nombre y el nombre de tu colmado. Sin papeleos, sin tarjeta de crédito para empezar.' },
              { n: '02', i: 'icon-[ic--baseline-inventory-2]', t: 'Carga tu inventario', d: 'Agrega tus productos con precio y cantidad. Puedes importar desde Excel. Sencillo como anotar en la libreta.' },
              { n: '03', i: 'icon-[ic--baseline-point-of-sale]', t: 'Empieza a vender', d: 'Abre el POS, selecciona productos y cobra. El sistema registra ventas, fiados y cuadre automáticamente.' },
              { n: '04', i: 'icon-[ic--baseline-analytics]', t: 'Ve tus reportes', d: 'Al final del turno el sistema te dice exactamente cuánto dinero debe haber en la gaveta. Sin sorpresas.' }
            ]" :key="i" class="p-8 rounded-2xl bg-[#0F1F3D] border border-blue-900/20 transition-all duration-300 hover:-translate-y-1 hover:border-blue-500/50 group">
              <div class="font-bebas text-5xl text-[#1E3A8A] mb-4 transition-colors group-hover:text-blue-500/30">{{ step.n }}</div>
              <div class="w-12 h-12 rounded-xl bg-blue-500/5 flex items-center justify-center mb-6 group-hover:bg-blue-500/10 transition-colors">
                <span :class="[step.i, 'text-3xl text-blue-500/50 group-hover:text-blue-400 transition-all']"></span>
              </div>
              <h4 class="text-xl font-bold text-white mb-3">{{ step.t }}</h4>
              <p class="text-slate-400 text-sm leading-relaxed">{{ step.d }}</p>
            </div>
          </div>
        </section>

        <!-- SECCIÓN B: DEMO / VIDEO -->
        <section class="max-w-4xl mx-auto mb-48 text-center bg-[#080E1A] p-12 md:p-20 rounded-[3rem] border border-white/5 shadow-2xl relative overflow-hidden">
          <div class="absolute inset-0 bg-[radial-gradient(circle_at_50%_50%,rgba(37,99,235,0.05),transparent)]"></div>
          
          <div class="relative z-10">
            <span class="px-4 py-1.5 rounded-full border border-blue-600/30 bg-blue-600/5 text-blue-400 text-xs font-black uppercase tracking-[0.2em] mb-6 inline-block">MÍRALO EN ACCIÓN</span>
            <h2 class="text-6xl md:text-8xl font-bebas text-white mb-6 uppercase tracking-widest leading-none">Ve el sistema antes de comprar</h2>
            <p class="text-xl text-slate-400 mb-12 max-w-2xl mx-auto">
              Un demo real del POS. Sin actores, sin scripts. Así trabajan los colmaderos que ya usan TuColmadoRD.
            </p>

            <div class="relative aspect-video w-full rounded-2xl border border-blue-900/40 overflow-hidden bg-slate-900 group shadow-[0_0_100px_-20px_rgba(0,0,0,0.8)]">
              <template v-if="!showVideo">
                <div class="absolute inset-0 flex items-center justify-center cursor-pointer" @click="showVideo = true">
                  <div class="absolute inset-0 bg-black/60 z-10 group-hover:bg-black/40 transition-colors"></div>
                  <div class="relative z-20 w-24 h-24 bg-blue-600 rounded-full flex items-center justify-center shadow-[0_0_0_16px_rgba(37,99,235,0.15)] transition-all duration-500 group-hover:scale-110 group-hover:shadow-[0_0_0_24px_rgba(37,99,235,0.1)]">
                    <span class="icon-[ic--baseline-play-arrow] text-5xl text-white ml-2"></span>
                  </div>
                  <span class="absolute bottom-10 text-white/40 font-bebas tracking-widest text-sm uppercase">Vista previa del POS</span>
                </div>
              </template>
              <template v-else>
                <iframe 
                  class="w-full h-full" 
                  src="https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=1" 
                  title="TuColmadoRD Demo" 
                  frameborder="0" 
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" 
                  allowfullscreen
                ></iframe>
              </template>
            </div>
            
            <div class="mt-8 flex items-center justify-center gap-2 text-blue-400 font-bold text-sm">
              <span class="w-1.5 h-1.5 bg-blue-400 rounded-full"></span>
              Demo de 3 minutos · Sin registro requerido
            </div>
          </div>
        </section>

        <!-- SECCIÓN C: ACCESO GRATUITO -->
        <section id="precios" class="max-w-4xl mx-auto mb-48 pt-20 scroll-mt-32">
          <div class="text-center mb-16 px-8">
            <span class="px-4 py-1.5 rounded-full border border-blue-500/30 bg-blue-500/5 text-blue-400 text-xs font-black uppercase tracking-[0.2em] mb-6 inline-block">TU INVERSIÓN EN TI</span>
            <h2 class="text-6xl md:text-8xl font-bebas text-white mb-10 uppercase tracking-widest leading-none">RD$0</h2>
          </div>

          <!-- Single Pricing Card -->
          <div class="relative p-12 md:p-16 rounded-[3rem] bg-gradient-to-br from-blue-500/10 via-slate-900/80 to-slate-900/80 border-2 border-blue-500/50 shadow-[0_0_60px_rgba(37,99,235,0.2)]">
            <div class="absolute -top-6 left-1/2 -translate-x-1/2 px-6 py-2 bg-blue-500 text-white rounded-full text-sm font-black uppercase tracking-[0.2em] shadow-xl">
              PROGRAMA PILOTO
            </div>

            <h3 class="text-4xl md:text-5xl font-bebas text-white mb-4 uppercase tracking-wider">Usuario Pionero</h3>
            <p class="text-lg text-slate-300 mb-12 font-medium">
              Buscamos a los primeros 20 clientes pioneros para construir TuColmadoRD juntos.
            </p>

            <div class="space-y-8 mb-16">
              <div class="flex items-start gap-6">
                <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center flex-shrink-0 text-white font-black text-xl shadow-lg shadow-blue-500/50">
                  ✓
                </div>
                <div>
                  <h4 class="text-xl font-black text-white mb-2">1 año de acceso completo</h4>
                  <p class="text-slate-400 text-sm">Sin límites de usuarios, facturación, inventario o fiados. Todo incluido hasta Abril 2027.</p>
                </div>
              </div>

              <div class="flex items-start gap-6">
                <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center flex-shrink-0 text-white font-black text-xl shadow-lg shadow-blue-500/50">
                  ✓
                </div>
                <div>
                  <h4 class="text-xl font-black text-white mb-2">Soporte dominicano por WhatsApp</h4>
                  <p class="text-slate-400 text-sm">Hablas con nosotros, no con un bot. Lunes a sábado. Entendemos tu colmado porque somos de aquí.</p>
                </div>
              </div>

              <div class="flex items-start gap-6">
                <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center flex-shrink-0 text-white font-black text-xl shadow-lg shadow-blue-500/50">
                  ✓
                </div>
                <div>
                  <h4 class="text-xl font-black text-white mb-2">Tu voz define el producto</h4>
                  <p class="text-slate-400 text-sm">Nuestros primeros 20 clientes deciden qué se construye. Si sugieres una feature y es necesaria, la hacemos.</p>
                </div>
              </div>

              <div class="flex items-start gap-6">
                <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center flex-shrink-0 text-white font-black text-xl shadow-lg shadow-blue-500/50">
                  ✓
                </div>
                <div>
                  <h4 class="text-xl font-black text-white mb-2">Sin tarjeta de crédito ahora</h4>
                  <p class="text-slate-400 text-sm">No pedimos forma de pago. Al completar el año, evaluaremos juntos el siguiente paso con precios preferenciales.</p>
                </div>
              </div>
            </div>

            <a :href="webAdminRegisterUrl" class="w-full py-5 text-center bg-blue-600 hover:bg-blue-500 text-white font-black rounded-xl transition-all text-sm uppercase tracking-widest shadow-[0_10px_30px_rgba(37,99,235,0.3)]">
              Solicitar mi lugar
            </a>

            <p class="text-center text-slate-500 text-xs mt-8 font-medium">
              Plazas limitadas a los primeros 20 clientes. Se asignan por orden de postulación.
            </p>
          </div>
        </section>

        <!-- SECCIÓN D: FAQ -->
        <section id="faq" class="max-w-3xl mx-auto mb-48 pt-20 scroll-mt-32">
          <div class="text-center mb-16">
            <h2 class="text-6xl md:text-8xl font-bebas text-white mb-10 uppercase tracking-widest">Preguntas frecuentes</h2>
          </div>

          <div class="space-y-4 mb-20">
            <div v-for="(faq, i) in faqs" :key="i" class="rounded-2xl border transition-all duration-300" :class="activeFaq === i ? 'bg-[#0D1B2E] border-blue-500/30' : 'bg-[#0b1222]/40 border-white/5'">
              <button 
                @click="toggleFaq(i)"
                class="w-full px-8 py-6 flex items-center justify-between text-left group"
              >
                <span class="text-lg font-bold text-white pr-8">{{ faq.p }}</span>
                <div 
                  class="w-8 h-8 rounded-full border border-white/10 flex items-center justify-center transition-all duration-300 group-hover:border-blue-500/50"
                  :class="activeFaq === i ? 'rotate-45 border-blue-500 bg-blue-500/10' : ''"
                >
                  <span class="icon-[ic--baseline-add] text-white transition-colors" :class="activeFaq === i ? 'text-blue-400' : ''"></span>
                </div>
              </button>
              <div v-show="activeFaq === i" class="px-8 pb-8 animate-in fade-in slide-in-from-top-2 duration-300">
                <p class="text-slate-400 leading-relaxed">{{ faq.r }}</p>
              </div>
            </div>
          </div>

          <div class="bg-blue-600/5 border border-blue-500/20 p-10 rounded-3xl text-center group transition-all hover:bg-blue-600/10">
            <h4 class="text-xl font-bold text-white mb-6">¿Tienes otra pregunta? Escríbenos directo por WhatsApp.</h4>
            <a href="https://wa.me/18296932458" target="_blank" class="inline-flex items-center gap-3 px-8 py-4 bg-blue-600 hover:bg-blue-500 text-white rounded-xl font-black uppercase tracking-widest text-xs transition-all shadow-[0_10px_20px_rgba(37,99,235,0.2)]">
              <span class="icon-[ic--baseline-whatsapp] text-xl"></span>
              Hablar con Soporte
            </a>
          </div>
        </section>

        <!-- Warranty Section: Pattern & Highlight -->
        <div class="max-w-6xl mx-auto mb-48 relative overflow-hidden rounded-[4rem] border border-white/10">
          <div class="absolute inset-0 bg-[#0F172A]"></div>
          <!-- Geometric Pattern -->
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
      </div>
    </main>

    <footer class="bg-[#080E1A] border-t border-white/5 pt-32 pb-16 relative z-10">
      <div class="max-w-7xl mx-auto px-8">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-24 mb-24 text-left">
          <div class="col-span-1 md:col-span-2">
            <div class="flex items-center gap-4 mb-10 group cursor-pointer">
              <div class="relative">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="w-10 h-10 transition-transform duration-500 group-hover:rotate-12" fill="none" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                  <rect x="3" y="3" width="12" height="12" rx="2" stroke="#2563eb" />
                  <rect x="9" y="9" width="12" height="12" rx="2" stroke="#dc2626" />
                </svg>
                <div class="absolute inset-0 bg-blue-500/15 blur-xl rounded-full scale-0 group-hover:scale-150 transition-transform duration-500"></div>
              </div>
              <div class="flex flex-col leading-none">
                <span class="text-2xl font-bebas tracking-wider text-white leading-none">TuColmado<span class="text-blue-500">RD</span></span>
                <span class="text-[0.55rem] font-bold text-slate-600 uppercase tracking-[0.35em] mt-1">by Synset Solutions</span>
              </div>
            </div>
            <p class="text-slate-400 text-lg leading-relaxed max-w-md">
              La plataforma definitiva diseñada en y para República Dominicana. Digitalizamos el corazón de nuestros barrios con orgullo y tecnología de punta.
            </p>
            <div class="flex gap-6 mt-12">
              <a href="#" class="w-14 h-14 rounded-2xl bg-white/5 border border-white/5 flex items-center justify-center hover:bg-blue-600 hover:border-blue-500 hover:text-white transition-all text-slate-400 group">
                <span class="icon-[ic--baseline-facebook] text-2xl group-hover:scale-110 transition-transform"></span>
              </a>
              <a href="https://wa.me/18296932458" class="w-14 h-14 rounded-2xl bg-white/5 border border-white/5 flex items-center justify-center hover:bg-blue-600 hover:border-blue-500 hover:text-white transition-all text-slate-400 group">
                <span class="icon-[ic--baseline-whatsapp] text-2xl group-hover:scale-110 transition-transform"></span>
              </a>
              <a href="#" class="w-14 h-14 rounded-2xl bg-white/5 border border-white/5 flex items-center justify-center hover:bg-blue-600 hover:border-blue-500 hover:text-white transition-all text-slate-400 group">
                <span class="icon-[ic--baseline-email] text-2xl group-hover:scale-110 transition-transform"></span>
              </a>
            </div>
          </div>
          <div>
            <h4 class="text-white font-bebas text-2xl mb-10 tracking-widest uppercase opacity-80">Ecosistema</h4>
            <ul class="space-y-6 text-slate-400 text-lg">
              <li><a href="#" class="hover:text-blue-500 transition-colors">Sistema POS</a></li>
              <li><a href="#" class="hover:text-blue-500 transition-colors">App Móvil (Próximamente)</a></li>
              <li><a href="#" class="hover:text-blue-500 transition-colors">Proveedores B2B</a></li>
            </ul>
          </div>
          <div>
            <h4 class="text-white font-bebas text-2xl mb-10 tracking-widest uppercase opacity-80">Soporte</h4>
            <ul class="space-y-6 text-slate-400 text-lg">
              <li><a href="#faq" class="hover:text-blue-500 transition-colors">Preguntas Frecuentes</a></li>
              <li><a href="https://wa.me/18296932458" class="hover:text-blue-500 transition-colors">WhatsApp 24/7</a></li>
              <li><a href="#" class="hover:text-blue-500 transition-colors">Política de Privacidad</a></li>
            </ul>
          </div>
        </div>
        <div class="pt-16 border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-6">
          <p class="text-slate-600 text-xs font-bold uppercase tracking-widest">
            © 2026 TuColmado RD &nbsp;·&nbsp; Orgullosamente Dominicano 🇩🇴
          </p>
          <a href="https://synsetsolutions.com" target="_blank" rel="noopener noreferrer"
             class="flex items-center gap-3 opacity-35 hover:opacity-80 transition-all group">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="w-5 h-5 group-hover:rotate-12 transition-transform duration-500" fill="none" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="12" height="12" rx="2" stroke="#2563eb" />
              <rect x="9" y="9" width="12" height="12" rx="2" stroke="#dc2626" />
            </svg>
            <div class="flex flex-col leading-none">
              <span class="text-[0.55rem] font-bold text-slate-500 uppercase tracking-[0.35em]">Desarrollado por</span>
              <span class="text-xs font-black text-slate-300 group-hover:text-blue-400 transition-colors uppercase tracking-[0.2em]">Synset Solutions</span>
            </div>
          </a>
        </div>
      </div>
    </footer>
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Bebas+Neue&family=Outfit:wght@100..900&display=swap');

@keyframes width-grow {
  0% { width: 0; left: 0; }
  50% { width: 100%; left: 0; }
  100% { width: 0; left: 100%; }
}

.animate-marquee {
  display: flex;
  width: fit-content;
  animation: marquee 30s linear infinite;
}

@keyframes marquee {
  0% { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}

html {
  scroll-behavior: smooth;
}

body {
  margin: 0;
  padding: 0;
}
</style>
