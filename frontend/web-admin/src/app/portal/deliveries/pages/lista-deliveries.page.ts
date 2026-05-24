import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { DeliveriesService } from '../services/deliveries.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { DeliveryPendiente } from '../models/delivery.model';

import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';

@Component({
  selector: 'app-lista-deliveries',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [DatePipe, CardComponent, TableComponent, SpinnerComponent, BtnComponent, BadgeComponent],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Deliveries</h2>
        <button appBtn variant="ghost" size="sm" (click)="loadPendientes()">
          <iconify-icon icon="lucide:refresh-cw" class="text-base"></iconify-icon>
          Actualizar
        </button>
      </div>

      <!-- Table -->
      <app-card>
        @if (loading()) {
          <div class="flex justify-center py-16">
            <app-spinner size="lg" />
          </div>
        } @else if (pendientes().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 gap-3">
            <iconify-icon icon="lucide:bike" class="text-6xl text-base-content/20"></iconify-icon>
            <div class="text-center">
              <p class="font-medium text-base-content/60">Sin deliveries pendientes</p>
              <p class="text-sm text-base-content/40 mt-1">Los pedidos a domicilio aparecerán aquí</p>
            </div>
          </div>
        } @else {
          <app-table>
            <thead>
              <tr>
                <th>Recibo</th>
                <th class="hidden sm:table-cell">Cliente</th>
                <th>Dirección</th>
                <th class="hidden md:table-cell">Fecha</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              @for (d of pendientes(); track d.orderId) {
                <tr class="hover">
                  <td class="font-mono text-sm font-medium text-base-content">{{ d.receiptNumber }}</td>
                  <td class="hidden sm:table-cell text-sm text-base-content/60">{{ d.customerName ?? 'Sin nombre' }}</td>
                  <td class="text-sm text-base-content/60 max-w-48 truncate">{{ d.address }}</td>
                  <td class="hidden md:table-cell text-sm text-base-content/60">{{ d.createdAt | date:'d MMM, HH:mm' }}</td>
                  <td>
                    <app-badge variant="warning">{{ d.status }}</app-badge>
                  </td>
                </tr>
              }
            </tbody>
          </app-table>
        }
      </app-card>
    </div>
  `,
})
export class ListaDeliveriesPage {
  private svc = inject(DeliveriesService);
  private toast = inject(ToastService);

  loading = signal(true);
  pendientes = signal<DeliveryPendiente[]>([]);

  constructor() { this.loadPendientes(); }

  loadPendientes(): void {
    this.loading.set(true);
    this.svc.getPendientes().subscribe({
      next: (items) => { this.pendientes.set(items); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando deliveries'); this.loading.set(false); },
    });
  }

}
