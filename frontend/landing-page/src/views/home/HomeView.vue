<script setup lang="ts">
import { useReveal } from '@/composables/useReveal'
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

useReveal()
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
