import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';

@Component({
  selector: 'app-tc-logo',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [class]="'flex items-center gap-2 select-none ' + extraClass()">

      <!-- Mismo SVG que AppLogo.vue en tucolmadord-landing -->
      <svg
        [attr.class]="svgClass()"
        viewBox="0 0 48 48"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        aria-hidden="true"
      >
        <!-- Caja azul -->
        <rect x="2" y="2" width="28" height="28" rx="6"
              class="stroke-primary" stroke-width="4" fill="none" />
        <text x="16" y="16"
              text-anchor="middle" dominant-baseline="central"
              class="fill-secondary"
              style="font-family:'Playfair Display',serif;font-weight:900;font-size:20px">T</text>

        <!-- Caja roja -->
        <rect x="18" y="18" width="28" height="28" rx="6"
              class="stroke-secondary" stroke-width="4" fill="none" />
        <text x="32" y="32"
              text-anchor="middle" dominant-baseline="central"
              class="fill-primary"
              style="font-family:'Playfair Display',serif;font-weight:900;font-size:20px">C</text>
      </svg>

      @if (showName()) {
        <div class="flex items-baseline font-serif font-black tracking-tighter" [class]="textClass()">
          <span class="text-secondary">Tu</span>
          <span [class]="onDark() ? 'text-white' : 'text-base-content'" class="mx-0.5">Colmado</span>
          <span class="text-secondary">R</span><span class="text-primary -ml-1">D</span>
        </div>
      }
    </div>
  `,
})
export class TcLogoComponent {
  size      = input<'sm' | 'md' | 'lg'>('md');
  showName  = input(true);
  onDark    = input(false);
  extraClass = input('');

  svgClass = computed(() => {
    const s = { sm: 'w-7 h-7', md: 'w-10 h-10', lg: 'w-14 h-14' };
    return `${s[this.size()]} overflow-visible shrink-0`;
  });

  textClass = computed(() => {
    const s = { sm: 'text-xl', md: 'text-2xl', lg: 'text-3xl' };
    return s[this.size()];
  });
}
