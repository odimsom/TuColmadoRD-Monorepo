<script setup lang="ts">
import { onMounted } from 'vue'
import Nvar from '@/components/layout/Nvar.vue'
import HeroSection from '@/views/home/sections/Hero.vue'
import ProductShowcaseSection from '@/views/home/sections/ProductShowcase.vue'
import FeaturesSection from '@/views/home/sections/Features.vue'
import AntesDespuesSection from '@/views/home/sections/AntesDespues.vue'
import HowItWorksSection from '@/views/home/sections/HowItWorks.vue'
import PricingSection from '@/views/home/sections/Pricing.vue'
import SocialProofSection from '@/views/home/sections/SocialProof.vue'
import FAQSection from '@/views/home/sections/FAQ.vue'
import CtaSection from '@/views/home/sections/Cta.vue'
import FooterLayout from '@/components/layout/Footer.vue'

onMounted(() => {
  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('active')
        }
      })
    },
    { threshold: 0.12, rootMargin: '0px 0px -40px 0px' },
  )

  document.querySelectorAll('.reveal, .reveal-left, .reveal-right').forEach((el) => {
    observer.observe(el)
  })
})
</script>

<template>
  <div class="min-h-screen w-full flex flex-col bg-base-100">
    <Nvar />
    <main>
      <HeroSection />
      <ProductShowcaseSection />
      <FeaturesSection />
      <AntesDespuesSection />
      <HowItWorksSection />
      <PricingSection />
      <SocialProofSection />
      <FAQSection />
      <CtaSection />
    </main>
    <FooterLayout />
  </div>
</template>
