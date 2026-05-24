import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { VentasService } from '../services/ventas.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { SALE_STATUS, VentaResumen } from '../models/venta.model';

@Component({
  selector: 'app-lista-ventas',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, DatePipe, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, RdCurrencyPipe, BtnComponent],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Ventas</h2>
        @if (totalRevenue() > 0) {
          <div class="text-right">
            <p class="text-xs font-bold text-base-content/40 uppercase tracking-widest">Ingresos totales</p>
            <p class="text-xl font-black text-primary">{{ totalRevenue() | rdCurrency }}</p>
          </div>
        }
      </div>

      <!-- Table -->
      <app-card>
        @if (loading()) {
          <div class="flex justify-center py-16">
            <app-spinner size="lg" />
          </div>
        } @else if (ventas().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 gap-3">
            <iconify-icon icon="lucide:shopping-cart" class="text-6xl text-base-content/20"></iconify-icon>
            <p class="text-sm text-base-content/40">No hay ventas registradas</p>
          </div>
        } @else {
          <app-table>
            <thead>
              <tr>
                <th>Recibo</th>
                <th class="hidden sm:table-cell">Fecha</th>
                <th class="hidden md:table-cell">Artículos</th>
                <th>Total</th>
                <th>Estado</th>
                <th class="w-8"></th>
              </tr>
            </thead>
            <tbody>
              @for (v of ventas(); track v.saleId) {
                <tr class="hover cursor-pointer" [routerLink]="[v.saleId]">
                  <td class="font-mono text-sm font-medium text-base-content">{{ v.receiptNumber }}</td>
                  <td class="hidden sm:table-cell text-sm text-base-content/60">{{ v.createdAt | date:'d MMM y, HH:mm' }}</td>
                  <td class="hidden md:table-cell text-sm text-base-content/60">{{ v.itemCount }}</td>
                  <td class="font-semibold text-base-content">{{ v.total | rdCurrency }}</td>
                  <td>
                    <app-badge [variant]="getStatusVariant(v.statusId)">
                      {{ getStatusLabel(v.statusId) }}
                    </app-badge>
                  </td>
                  <td>
                    <iconify-icon icon="lucide:chevron-right" class="text-base-content/30"></iconify-icon>
                  </td>
                </tr>
              }
            </tbody>
          </app-table>

          @if (totalPages() > 1) {
            <div class="flex items-center justify-between px-4 py-3 border-t border-base-300">
              <p class="text-sm text-base-content/50">{{ totalCount() }} venta{{ totalCount() !== 1 ? 's' : '' }}</p>
              <div class="join">
                <button appBtn size="sm" class="join-item" [disabled]="page() === 1" (click)="changePage(page() - 1)">
                  <iconify-icon icon="lucide:chevron-left"></iconify-icon>
                </button>
                <span class="join-item tc-btn tc-btn-sm tc-btn-ghost pointer-events-none">{{ page() }} / {{ totalPages() }}</span>
                <button appBtn size="sm" class="join-item" [disabled]="page() === totalPages()" (click)="changePage(page() + 1)">
                  <iconify-icon icon="lucide:chevron-right"></iconify-icon>
                </button>
              </div>
            </div>
          }
        }
      </app-card>
    </div>
  `,
})
export class ListaVentasPage {
  private svc = inject(VentasService);
  private toast = inject(ToastService);

  loading = signal(true);
  ventas = signal<VentaResumen[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  totalRevenue = signal(0);
  page = signal(1);

  constructor() { this.loadVentas(); }

  loadVentas(): void {
    this.loading.set(true);
    this.svc.getVentas(this.page(), 20).subscribe({
      next: (res) => {
        this.ventas.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.totalRevenue.set(res.totalRevenue);
        this.loading.set(false);
      },
      error: () => { this.toast.error('Error cargando ventas'); this.loading.set(false); },
    });
  }

  changePage(p: number): void { this.page.set(p); this.loadVentas(); }

  getStatusLabel(statusId: number): string {
    return SALE_STATUS[statusId]?.label ?? 'Desconocido';
  }

  getStatusVariant(statusId: number): 'success' | 'error' | 'neutral' {
    return (SALE_STATUS[statusId]?.variant as 'success' | 'error' | 'neutral') ?? 'neutral';
  }
}
