import { Component, ChangeDetectionStrategy, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-clientes-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Clientes</h2>
      </div>
      <div class="flex flex-col items-center justify-center py-24 gap-4">
        <iconify-icon icon="lucide:users" class="text-5xl text-base-content/20"></iconify-icon>
        <p class="text-base-content/40 text-sm">Módulo en construcción</p>
      </div>
    </div>
  `,
})
export class ListaClientesPage {}
