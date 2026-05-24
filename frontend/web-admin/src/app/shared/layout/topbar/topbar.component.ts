import { Component, ChangeDetectionStrategy, inject, output, CUSTOM_ELEMENTS_SCHEMA, input } from '@angular/core';
import { ThemeService } from '../../../core/theme.service';

@Component({
  selector: 'app-topbar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <header class="h-14 bg-base-100 border-b border-base-300 flex items-center justify-between px-4 lg:px-6 shrink-0">

      <!-- Left: mobile menu + page title -->
      <div class="flex items-center gap-3">
        <button
          class="btn btn-ghost btn-sm btn-square lg:hidden"
          (click)="menuToggle.emit()"
          aria-label="Abrir menú"
        >
          <iconify-icon icon="lucide:menu" class="text-lg"></iconify-icon>
        </button>
        <h1 class="text-base font-semibold text-base-content">{{ title() }}</h1>
      </div>

      <!-- Right: actions -->
      <div class="flex items-center gap-1">
        <button
          class="btn btn-ghost btn-sm btn-square"
          (click)="themeService.toggle()"
          [attr.aria-label]="themeService.isDark() ? 'Modo claro' : 'Modo oscuro'"
        >
          @if (themeService.isDark()) {
            <iconify-icon icon="lucide:sun" class="text-lg"></iconify-icon>
          } @else {
            <iconify-icon icon="lucide:moon" class="text-lg"></iconify-icon>
          }
        </button>
      </div>
    </header>
  `,
})
export class TopbarComponent {
  themeService = inject(ThemeService);
  title = input('');
  menuToggle = output<void>();
}
