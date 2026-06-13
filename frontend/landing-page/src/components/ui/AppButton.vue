<script setup lang="ts">
type Variant = 'primary' | 'secondary' | 'accent' | 'neutral' | 'ghost' | 'outline' | 'error' | 'warning' | 'success' | 'info'
type Size = 'xs' | 'sm' | 'md' | 'lg' | 'xl'

const props = withDefaults(defineProps<{
  variant?: Variant
  size?: Size
  href?: string
  disabled?: boolean
  wide?: boolean
}>(), {
  variant: 'primary',
  size: 'md',
})

const variantClass: Record<Variant, string> = {
  primary:   'btn-primary',
  secondary: 'btn-secondary',
  accent:    'btn-accent',
  neutral:   'btn-neutral',
  ghost:     'btn-ghost',
  outline:   'btn-outline',
  error:     'btn-error',
  warning:   'btn-warning',
  success:   'btn-success',
  info:      'btn-info',
}

const sizeClass: Record<Size, string> = {
  xs: 'btn-xs',
  sm: 'btn-sm',
  md: '',
  lg: 'btn-lg',
  xl: 'btn-xl',
}
</script>

<template>
  <component
    :is="href ? 'a' : 'button'"
    :href="href"
    :disabled="!href && disabled ? true : undefined"
    :aria-disabled="href && disabled ? true : undefined"
    class="btn"
    :class="[variantClass[variant], sizeClass[size], { 'btn-wide': wide, 'btn-disabled': disabled }]"
  >
    <slot />
  </component>
</template>
