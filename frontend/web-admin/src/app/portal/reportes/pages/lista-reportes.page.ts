import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { ReportesService } from '../services/reportes.service';
import { ReporteVentas, ReporteInventario, ReporteClientes } from '../models/reporte.model';

function isoDate(d: Date): string {
  return d.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-lista-reportes',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, CardComponent, SpinnerComponent, RdCurrencyPipe],
  template: `
    <div class="space-y-5">

      <div class="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h2 class="text-2xl font-black text-base-content tracking-tight">Reportes</h2>
          <p class="text-sm text-base-content/50 mt-1">Ventas, inventario y fiados del negocio</p>
        </div>
        <div class="flex items-end gap-2">
          <div class="form-control">
            <label class="label pb-1" for="rep-from"><span class="text-xs font-bold uppercase tracking-wider">Desde</span></label>
            <input id="rep-from" type="date" class="tc-input" [value]="from()" (change)="from.set($any($event.target).value)" />
          </div>
          <div class="form-control">
            <label class="label pb-1" for="rep-to"><span class="text-xs font-bold uppercase tracking-wider">Hasta</span></label>
            <input id="rep-to" type="date" class="tc-input" [value]="to()" (change)="to.set($any($event.target).value)" />
          </div>
          <button class="tc-btn tc-btn-primary" (click)="loadAll()">
            <iconify-icon icon="lucide:refresh-cw"></iconify-icon>
            Actualizar
          </button>
        </div>
      </div>

      @if (loading()) {
        <div class="flex justify-center py-16"><app-spinner size="lg" /></div>
      } @else {

        <!-- Resumen de ventas -->
        @if (ventas(); as v) {
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div class="tc-card p-5">
              <p class="text-xs text-base-content/40 uppercase tracking-widest font-black">Ingresos del período</p>
              <p class="text-3xl font-black text-primary mt-1">{{ v.total_revenue | rdCurrency }}</p>
            </div>
            <div class="tc-card p-5">
              <p class="text-xs text-base-content/40 uppercase tracking-widest font-black">Transacciones</p>
              <p class="text-3xl font-black mt-1">{{ v.transaction_count }}</p>
            </div>
            <div class="tc-card p-5">
              <p class="text-xs text-base-content/40 uppercase tracking-widest font-black">Ticket promedio</p>
              <p class="text-3xl font-black mt-1">{{ v.average_ticket | rdCurrency }}</p>
            </div>
          </div>

          <app-card>
            <h3 class="font-black uppercase tracking-tight mb-3">Productos más vendidos</h3>
            @if (v.top_products.length === 0) {
              <p class="text-sm text-base-content/40 py-6 text-center">Sin ventas en el período seleccionado</p>
            } @else {
              <div class="space-y-2">
                @for (p of v.top_products; track p.product_name; let i = $index) {
                  <div class="flex items-center gap-3">
                    <span class="w-6 text-center font-black text-base-content/30">{{ i + 1 }}</span>
                    <div class="flex-1">
                      <p class="text-sm font-bold">{{ p.product_name }}</p>
                      <p class="text-xs text-base-content/40">{{ p.units_sold }} unidades</p>
                    </div>
                    <p class="font-black">{{ p.revenue | rdCurrency }}</p>
                  </div>
                }
              </div>
            }
          </app-card>
        } @else {
          <div class="tc-card p-6 text-center text-sm text-base-content/40">
            No se pudo cargar el reporte de ventas. Intenta actualizar.
          </div>
        }

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 items-start">
          <!-- Fiados -->
          @if (clientes(); as c) {
            <app-card>
              <div class="flex items-center justify-between mb-3">
                <h3 class="font-black uppercase tracking-tight">Fiados</h3>
                <a routerLink="/portal/clientes" class="text-xs text-primary font-bold uppercase">Ver clientes</a>
              </div>
              <div class="grid grid-cols-3 gap-3 text-center">
                <div><p class="text-2xl font-black">{{ c.total_customers }}</p><p class="text-xs text-base-content/40 uppercase">Clientes</p></div>
                <div><p class="text-2xl font-black">{{ c.with_debt }}</p><p class="text-xs text-base-content/40 uppercase">Con deuda</p></div>
                <div><p class="text-2xl font-black text-secondary">{{ c.total_debt | rdCurrency }}</p><p class="text-xs text-base-content/40 uppercase">Deuda total</p></div>
              </div>
            </app-card>
          }

          <!-- Stock bajo -->
          @if (inventario(); as inv) {
            <app-card>
              <div class="flex items-center justify-between mb-3">
                <h3 class="font-black uppercase tracking-tight">Alertas de stock</h3>
                <a routerLink="/portal/inventario" class="text-xs text-primary font-bold uppercase">Ver inventario</a>
              </div>
              @if (inv.low_stock.length === 0) {
                <p class="text-sm text-base-content/40 py-6 text-center">Todo el inventario está en niveles saludables</p>
              } @else {
                <div class="space-y-2 max-h-64 overflow-y-auto">
                  @for (a of inv.low_stock; track a.product_id) {
                    <div class="flex items-center justify-between gap-2">
                      <div class="min-w-0">
                        <p class="text-sm font-bold truncate">{{ a.product_name }}</p>
                        @if (a.category_name) {
                          <p class="text-xs text-base-content/40">{{ a.category_name }}</p>
                        }
                      </div>
                      <span class="tc-badge tc-badge-error shrink-0">{{ a.stock_quantity }} restantes</span>
                    </div>
                  }
                </div>
              }
            </app-card>
          }
        </div>
      }
    </div>
  `,
})
export class ListaReportesPage {
  private svc = inject(ReportesService);
  private toast = inject(ToastService);

  loading = signal(true);
  ventas = signal<ReporteVentas | null>(null);
  inventario = signal<ReporteInventario | null>(null);
  clientes = signal<ReporteClientes | null>(null);

  from = signal(isoDate(new Date(Date.now() - 29 * 86_400_000)));
  to = signal(isoDate(new Date()));

  constructor() { this.loadAll(); }

  loadAll(): void {
    this.loading.set(true);
    let pending = 3;
    const done = () => { if (--pending === 0) this.loading.set(false); };

    this.svc.getVentas(this.from(), this.to()).subscribe({
      next: (v) => { this.ventas.set(v); done(); },
      error: () => { this.toast.error('Error cargando reporte de ventas'); done(); },
    });
    this.svc.getInventario().subscribe({
      next: (v) => { this.inventario.set(v); done(); },
      error: () => { this.inventario.set(null); done(); },
    });
    this.svc.getClientes().subscribe({
      next: (v) => { this.clientes.set(v); done(); },
      error: () => { this.clientes.set(null); done(); },
    });
  }
}
