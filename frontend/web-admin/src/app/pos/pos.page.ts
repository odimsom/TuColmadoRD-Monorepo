import { Component, ChangeDetectionStrategy, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-pos-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="min-h-[calc(100vh-var(--topbar-h)-48px)] bg-base-200 flex items-center justify-center p-6">
      <div class="tc-card max-w-md w-full text-center p-12 shadow-2xl">
        <div class="w-16 h-16 bg-primary/10 flex items-center justify-center rounded-full mx-auto mb-6">
          <iconify-icon icon="lucide:scan-barcode" class="text-primary text-3xl"></iconify-icon>
        </div>
        <h1 class="text-3xl font-black text-base-content tracking-tighter italic uppercase leading-none mb-3">
          Punto de Venta
        </h1>
        <p class="text-base-content/40 text-sm font-medium uppercase tracking-widest mb-8">
          Próximamente disponible
        </p>
        <div class="tc-badge tc-badge-primary !rounded-md !py-2 px-4 italic font-bold">
          En construcción activa
        </div>
      </div>
    </div>
  `,
})
export class PosPage {}
