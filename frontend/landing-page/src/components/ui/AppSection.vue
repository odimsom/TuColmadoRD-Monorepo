<script setup lang="ts">
import { Icon } from '@iconify/vue'
import AppFlag from '@/components/ui/AppFlag.vue'

interface Feature {
  icon: string
  title: string
}

interface Props {
  image: string
  imageSide?: 'left' | 'right'
  flagText: string
  flagVariant?: 'primary' | 'secondary'
  title: string
  highlight?: string
  description: string
  benefits?: string[]
  features?: Feature[]
}

withDefaults(defineProps<Props>(), {
  imageSide: 'left',
  flagVariant: 'primary',
})
</script>

<template>
  <div :class="['flex flex-col lg:flex-row min-h-screen overflow-hidden mb-24 lg:mb-48 last:mb-0', imageSide === 'right' ? 'lg:flex-row-reverse' : '']">
    <div class="lg:w-1/2 relative min-h-[500px] lg:min-h-full overflow-hidden reveal">
      <img :src="image" class="absolute inset-0 w-full h-full object-cover scale-125 transition-transform duration-[3s] hover:scale-100" :alt="title" />
      <div :class="['absolute inset-0 mix-blend-multiply opacity-40', flagVariant === 'primary' ? 'bg-primary' : 'bg-secondary']"></div>
      <div :class="['absolute bottom-0 w-32 h-32 bg-base-100 hidden lg:block', imageSide === 'left' ? 'right-0 clip-flag-right' : 'left-0 clip-flag-left']"></div>
    </div>

    <div :class="['lg:w-1/2 flex flex-col justify-center p-12 lg:p-20 xl:p-32 relative bg-base-100', imageSide === 'right' ? 'items-end text-right' : '']">
      <div :class="['absolute top-12 hidden lg:block', imageSide === 'right' ? 'right-0 translate-x-16' : 'left-0 -translate-x-16']">
        <AppFlag :side="imageSide === 'right' ? 'right' : 'left'" :variant="flagVariant" class="reveal">
          {{ flagText }}
        </AppFlag>
      </div>

      <div :class="['max-w-xl reveal', imageSide === 'right' ? 'flex flex-col items-end' : '']">
        <h3 class="text-5xl md:text-7xl font-black text-base-content mb-6 leading-[0.85] tracking-tighter uppercase">
          {{ title }} <br />
          <span :class="['italic', flagVariant === 'primary' ? 'text-primary' : 'text-secondary']">{{ highlight }}</span>
        </h3>

        <p class="text-lg text-base-content/60 leading-relaxed mb-10 font-medium">
          {{ description }}
        </p>

        <div v-if="features" :class="['grid grid-cols-1 md:grid-cols-2 gap-6 mb-10 reveal-stagger', imageSide === 'right' ? 'text-right' : '']">
          <div
            v-for="item in features"
            :key="item.title"
            :class="['flex items-start gap-4 reveal', imageSide === 'right' ? 'flex-row-reverse' : '']"
          >
            <div :class="['p-3 rounded-none shadow-lg shrink-0', flagVariant === 'primary' ? 'bg-primary text-primary-content' : 'bg-secondary text-secondary-content']">
              <Icon :icon="item.icon" class="w-5 h-5" />
            </div>
            <div>
              <h4 class="font-black uppercase tracking-widest text-xs text-base-content mb-1">{{ item.title }}</h4>
              <div :class="['h-1 w-8', flagVariant === 'primary' ? 'bg-primary' : 'bg-secondary', imageSide === 'right' ? 'ml-auto' : '']"></div>
            </div>
          </div>
        </div>

        <ul v-if="benefits" :class="['space-y-3 mb-10', imageSide === 'right' ? 'items-end' : '']">
          <li
            v-for="benefit in benefits"
            :key="benefit"
            :class="['flex items-center gap-3 font-bold uppercase tracking-widest text-xs text-base-content/50 reveal', imageSide === 'right' ? 'flex-row-reverse' : '']"
          >
            <Icon icon="lucide:check" :class="['w-4 h-4 shrink-0', flagVariant === 'primary' ? 'text-primary' : 'text-secondary']" />
            {{ benefit }}
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
