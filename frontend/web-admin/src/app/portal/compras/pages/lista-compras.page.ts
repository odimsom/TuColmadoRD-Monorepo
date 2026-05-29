import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ComprasService } from '../services/compras.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { CompraResumen } from '../models/compra.model';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';

@Component({
  selector: 'app-lista-compras',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, DatePipe, CardComponent, TableComponent, SpinnerComponent, RdCurrencyPipe, BtnComponent],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Compras</h2>
        <a appBtn size="sm" routerLink="/portal/compras/nueva">
          <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
          Nueva entrada de stock
        </a>
      </div>

      <!-- Table -->
      <app-card>
        @if (loading()) {
          <div class="flex justify-center py-16">
            <app-spinner size="lg" />
          </div>
        } @else if (compras().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 gap-3">
            <iconify-icon icon="lucide:truck" class="text-6xl text-base-content/20"></iconify-icon>
            <p class="text-sm text-base-content/40">No hay entradas de stock registradas</p>
          </div>
        } @else {
          <app-table>
            <thead>
              <tr>
                <th>Fecha</th>
                <th class="hidden sm:table-cell">Proveedor</th>
                <th class="hidden md:table-cell text-right">Líneas</th>
                <th class="text-right">Costo total</th>
              </tr>
            </thead>
            <tbody>
              @for (c of compras(); track c.id) {
                <tr class="hover">
                  <td class="text-sm font-medium text-base-content">{{ c.purchasedAt | date:'d MMM y' }}</td>
                  <td class="hidden sm:table-cell text-sm text-base-content/60">{{ c.supplierName ?? '—' }}</td>
                  <td class="hidden md:table-cell text-sm text-right text-base-content/60">{{ c.lineCount }}</td>
                  <td class="text-right font-black text-base-content italic">{{ c.totalCost | rdCurrency }}</td>
                </tr>
              }
            </tbody>
          </app-table>

          @if (totalPages() > 1) {
            <div class="flex items-center justify-between px-4 py-3 border-t border-base-300">
              <p class="text-sm text-base-content/50">{{ totalCount() }} entrada{{ totalCount() !== 1 ? 's' : '' }}</p>
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
export class ListaComprasPage {
  private svc = inject(ComprasService);
  private toast = inject(ToastService);

  loading = signal(true);
  compras = signal<CompraResumen[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  page = signal(1);

  constructor() { this.loadCompras(); }

  loadCompras(): void {
    this.loading.set(true);
    this.svc.getCompras(this.page(), 20).subscribe({
      next: (res) => {
        this.compras.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => { this.toast.error('Error cargando compras'); this.loading.set(false); },
    });
  }

  changePage(p: number): void { this.page.set(p); this.loadCompras(); }
}
