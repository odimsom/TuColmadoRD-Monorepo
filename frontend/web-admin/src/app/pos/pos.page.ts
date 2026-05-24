import { Component, ChangeDetectionStrategy, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-pos-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="min-h-screen bg-base-200 flex items-center justify-center">
      <div class="text-center space-y-3">
        <iconify-icon icon="lucide:scan-barcode" class="text-5xl text-base-content/20"></iconify-icon>
        <h1 class="text-2xl font-black text-base-content">Punto de Venta</h1>
        <p class="text-base-content/40 text-sm">En construcción</p>
      </div>
    </div>
  `,
})
export class PosPage {}
