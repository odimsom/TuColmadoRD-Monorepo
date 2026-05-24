import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { VentasService } from '../services/ventas.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { PAYMENT_METHOD_LABELS, SALE_STATUS, VentaDetalle } from '../models/venta.model';

@Component({
  selector: 'app-detalle-venta',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, DatePipe, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe],
  template: `
    <div class="space-y-5">

      <!-- Back + Header -->
      <div class="flex flex-col sm:flex-row sm:items-start gap-3">
        <button appBtn variant="ghost" size="sm" routerLink="/portal/ventas" class="self-start shrink-0">
          <iconify-icon icon="lucide:arrow-left" class="text-base"></iconify-icon>
          Ventas
        </button>
        @if (venta(); as v) {
          <div class="flex-1 flex items-start justify-between gap-3">
            <div>
              <h2 class="text-2xl font-black text-base-content tracking-tight font-mono">{{ v.receiptNumber }}</h2>
              <p class="text-sm text-base-content/50 mt-0.5">{{ v.createdAt | date:'d MMM y, HH:mm' }} · {{ v.cashierName }}</p>
            </div>
            <div class="flex items-center gap-2">
              <app-badge [variant]="getStatusVariant(v.statusId)">{{ getStatusLabel(v.statusId) }}</app-badge>
              @if (v.statusId === 1) {
                <button appBtn variant="error" size="sm" (click)="anularModal.open()">
                  <iconify-icon icon="lucide:ban" class="text-base"></iconify-icon>
                  Anular
                </button>
              }
            </div>
          </div>
        }
      </div>

      @if (loading()) {
        <div class="flex justify-center py-16"><app-spinner size="lg" /></div>
      } @else if (venta(); as v) {

        <!-- Items -->
        <app-card [compact]="true">
          <h3 class="font-bold text-base-content uppercase text-xs tracking-widest mb-3 px-1">Artículos</h3>
          <app-table>
            <thead>
              <tr>
                <th>Producto</th>
                <th class="hidden sm:table-cell text-right">Cantidad</th>
                <th class="hidden md:table-cell text-right">Precio unit.</th>
                <th class="text-right">Total</th>
              </tr>
            </thead>
            <tbody>
              @for (item of v.items; track item.productId) {
                <tr>
                  <td class="font-medium text-base-content">{{ item.productName }}</td>
                  <td class="hidden sm:table-cell text-sm text-right text-base-content/60">{{ item.quantity }}</td>
                  <td class="hidden md:table-cell text-sm text-right text-base-content/60">{{ item.unitPrice | rdCurrency }}</td>
                  <td class="text-right font-semibold text-base-content">{{ item.total | rdCurrency }}</td>
                </tr>
              }
            </tbody>
          </app-table>
        </app-card>

        <!-- Summary -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <app-card [compact]="true">
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest mb-3">Totales</h3>
            <dl class="space-y-2">
              <div class="flex justify-between text-sm">
                <dt class="text-base-content/60 font-medium">Subtotal</dt>
                <dd class="font-bold text-base-content">{{ v.subtotal | rdCurrency }}</dd>
              </div>
              <div class="flex justify-between text-sm">
                <dt class="text-base-content/60 font-medium">ITBIS</dt>
                <dd class="font-bold text-base-content">{{ v.totalItbis | rdCurrency }}</dd>
              </div>
              <div class="border-t border-base-300 my-2 pt-2"></div>
              <div class="flex justify-between items-baseline">
                <dt class="font-black text-base-content uppercase text-[11px] tracking-widest">Total</dt>
                <dd class="font-black text-2xl text-primary">{{ v.total | rdCurrency }}</dd>
              </div>
              <div class="flex justify-between text-sm pt-2">
                <dt class="text-base-content/60 font-medium">Pagado</dt>
                <dd class="font-bold text-base-content text-success">{{ v.totalPaid | rdCurrency }}</dd>
              </div>
              @if (v.changeDue > 0) {
                <div class="flex justify-between text-sm">
                  <dt class="text-base-content/60 font-medium">Cambio</dt>
                  <dd class="font-bold text-secondary italic">{{ v.changeDue | rdCurrency }}</dd>
                </div>
              }
            </dl>
          </app-card>

          <app-card [compact]="true">
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest mb-3">Pagos</h3>
            <ul class="space-y-2">
              @for (p of v.payments; track p.receivedAt) {
                <li class="flex justify-between text-sm">
                  <span class="text-base-content/60 font-medium">{{ getPaymentLabel(p.paymentMethodId) }}</span>
                  <span class="font-bold text-base-content">{{ p.amount | rdCurrency }}</span>
                </li>
              }
            </ul>
            @if (v.notes) {
              <div class="mt-4 pt-3 border-t border-base-300">
                <p class="text-[10px] text-base-content/40 uppercase tracking-widest font-black mb-1.5">Nota</p>
                <p class="text-sm text-base-content/70 italic leading-relaxed">{{ v.notes }}</p>
              </div>
            }
          </app-card>
        </div>

        @if (v.voidReason) {
          <div class="bg-secondary/10 border-l-4 border-secondary p-4 flex gap-3">
            <iconify-icon icon="lucide:ban" class="text-secondary text-xl shrink-0"></iconify-icon>
            <div>
              <p class="font-black text-secondary uppercase text-xs tracking-widest">Venta anulada</p>
              <p class="text-sm text-base-content/70 mt-1">{{ v.voidReason }}</p>
            </div>
          </div>
        }
      }
    </div>

    <!-- Anular Modal -->
    <app-modal #anularModal title="Anular venta">
      <form [formGroup]="anularForm" (ngSubmit)="onAnular(anularModal)" class="space-y-4">
        <div class="bg-warning/10 border-l-4 border-warning p-4 flex gap-3">
          <iconify-icon icon="lucide:triangle-alert" class="text-warning text-xl shrink-0"></iconify-icon>
          <p class="text-sm text-base-content/70">Esta acción no se puede deshacer. La venta quedará marcada como anulada permanentemente.</p>
        </div>
        <div class="form-control">
          <label class="label pb-1" for="void-reason">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Motivo de anulación *</span>
          </label>
          <textarea id="void-reason" class="tc-input min-h-[100px]" formControlName="voidReason" placeholder="Error en el pedido, devolución..."></textarea>
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="anularModal.close()">Cancelar</button>
          <button appBtn variant="error" type="submit" [loading]="saving()" [disabled]="anularForm.invalid">
            Confirmar anulación
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class DetalleVentaPage {
  private route = inject(ActivatedRoute);
  private svc = inject(VentasService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  private saleId = this.route.snapshot.paramMap.get('id') ?? '';

  loading = signal(true);
  saving = signal(false);
  venta = signal<VentaDetalle | null>(null);

  anularForm = this.fb.nonNullable.group({
    voidReason: ['', [Validators.required, Validators.minLength(5)]],
  });

  constructor() { this.loadVenta(); }

  loadVenta(): void {
    this.loading.set(true);
    this.svc.getVenta(this.saleId).subscribe({
      next: (v) => { this.venta.set(v); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando venta'); this.loading.set(false); },
    });
  }

  getStatusLabel(statusId: number): string { return SALE_STATUS[statusId]?.label ?? 'Desconocido'; }
  getStatusVariant(statusId: number): 'success' | 'error' | 'neutral' {
    return (SALE_STATUS[statusId]?.variant as 'success' | 'error' | 'neutral') ?? 'neutral';
  }
  getPaymentLabel(id: number): string { return PAYMENT_METHOD_LABELS[id] ?? 'Otro'; }

  onAnular(modal: ModalComponent): void {
    if (this.anularForm.invalid) return;
    this.saving.set(true);
    const { voidReason } = this.anularForm.getRawValue();
    this.svc.anularVenta(this.saleId, voidReason).subscribe({
      next: () => { this.toast.success('Venta anulada'); modal.close(); this.loadVenta(); this.saving.set(false); },
      error: () => { this.toast.error('Error al anular venta'); this.saving.set(false); },
    });
  }
}
