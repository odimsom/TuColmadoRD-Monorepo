<script setup lang="ts">
interface Props {
  class?: string
}
defineProps<Props>()
</script>

<template>
  <div :class="['flex items-center gap-2 select-none', $props.class]">
    <svg viewBox="0 0 48 48" class="w-10 h-10 overflow-visible" fill="none" xmlns="http://www.w3.org/2000/svg">
      <!-- Blue Box -->
      <rect x="2" y="2" width="28" height="28" rx="6" class="stroke-primary" stroke-width="4" fill="none" />
      <text x="16" y="16" text-anchor="middle" dominant-baseline="central" class="fill-secondary font-serif font-black" style="font-size: 20px;">T</text>

      <!-- Red Box -->
      <rect x="18" y="18" width="28" height="28" rx="6" class="stroke-secondary" stroke-width="4" fill="none" />
      <text x="32" y="32" text-anchor="middle" dominant-baseline="central" class="fill-primary font-serif font-black" style="font-size: 20px;">C</text>
    </svg>
    
    <div class="flex items-baseline font-serif font-black text-3xl tracking-tighter">
      <span class="text-secondary">Tu</span>
      <span class="text-neutral-content mx-0.5">Colmado</span>
      <span class="relative flex items-center">
        <span class="text-secondary">R</span>
        <span class="text-primary -ml-1">D</span>
      </span>
    </div>
  </div>
</template>
