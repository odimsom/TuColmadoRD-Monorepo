<script setup lang="ts">
interface Props {
  side?: 'left' | 'right'
  variant?: 'primary' | 'secondary'
  class?: string
}

withDefaults(defineProps<Props>(), {
  side: 'right',
  variant: 'primary',
})
</script>

<template>
  <div
    :class="[
      'py-6 px-12 shadow-2xl transition-all duration-1000 transform',
      side === 'left' ? 'clip-flag-left pr-20' : 'clip-flag-right pl-20',
      variant === 'primary' ? 'bg-primary text-primary-content' : 'bg-secondary text-secondary-content',
      $props.class
    ]"
  >
    <h2 class="text-2xl md:text-4xl font-black uppercase tracking-tighter italic whitespace-nowrap">
      <slot />
    </h2>
  </div>
</template>
