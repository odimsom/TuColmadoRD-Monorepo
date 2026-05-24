import { Component, ChangeDetectionStrategy, inject, output, CUSTOM_ELEMENTS_SCHEMA, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ThemeService } from '../../../core/theme.service';

@Component({
  selector: 'app-topbar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [DatePipe],
  template: `
    <header class="h-14 bg-base-100 border-b border-base-300 flex items-center justify-between px-4 lg:px-6 shrink-0 shadow-sm">

      <!-- Left: mobile menu + page title -->
      <div class="flex items-center gap-4">
        <button
          class="tc-btn tc-btn-ghost tc-btn-sm tc-btn-square lg:hidden"
          (click)="menuToggle.emit()"
          aria-label="Abrir menú"
        >
          <iconify-icon icon="lucide:menu" class="text-lg"></iconify-icon>
        </button>
        <h1 class="text-[15px] font-bold text-base-content tracking-tight">{{ title() }}</h1>
        <span class="hidden md:inline text-xs font-medium text-base-content/40 border-l border-base-300 pl-4">
          {{ today | date:'EEEE, d MMMM':'':'es-DO' }}
        </span>
      </div>

      <!-- Right: actions -->
      <div class="flex items-center gap-1">
        <button
          class="tc-btn tc-btn-ghost tc-btn-sm tc-btn-square"
          aria-label="Buscar"
        >
          <iconify-icon icon="lucide:search" class="text-lg"></iconify-icon>
        </button>
        <button
          class="tc-btn tc-btn-ghost tc-btn-sm tc-btn-square relative"
          aria-label="Notificaciones"
        >
          <iconify-icon icon="lucide:bell" class="text-lg"></iconify-icon>
          <span class="absolute top-2 right-2 w-2 h-2 bg-secondary rounded-full"></span>
        </button>
        <button
          class="tc-btn tc-btn-ghost tc-btn-sm tc-btn-square ml-1"
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
  today = new Date();
}
