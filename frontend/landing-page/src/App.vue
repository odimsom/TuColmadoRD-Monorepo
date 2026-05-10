<script setup lang="ts">
import AppHeader from './components/AppHeader.vue'
import HeroSection from './components/HeroSection.vue'
import MarqueeSection from './components/MarqueeSection.vue'
import FeatureCards from './components/FeatureCards.vue'
import HowItWorks from './components/HowItWorks.vue'
import VideoDemo from './components/VideoDemo.vue'
import PricingSection from './components/PricingSection.vue'
import FaqSection from './components/FaqSection.vue'
import WarrantySection from './components/WarrantySection.vue'
import AppFooter from './components/AppFooter.vue'
</script>

<template>
  <div class="min-h-screen flex flex-col relative overflow-hidden bg-[#0f172a] font-sans selection:bg-blue-500/30">
    <!-- Ambient Background -->
    <div class="absolute top-[-20%] left-[-10%] w-[500px] h-[500px] bg-blue-900/20 rounded-full blur-[120px] pointer-events-none"></div>
    <div class="absolute bottom-[-10%] right-[-5%] w-[500px] h-[500px] bg-blue-900/10 rounded-full blur-[100px] pointer-events-none"></div>

    <AppHeader />

    <main class="flex-grow flex flex-col pt-0">
      <HeroSection />
      <MarqueeSection />

      <div class="container mx-auto px-8 py-32">
        <FeatureCards />
        <HowItWorks />
        <VideoDemo />
        <PricingSection />
        <FaqSection />
        <WarrantySection />
      </div>
    </main>

    <AppFooter />
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
