import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ClientesService } from '../services/clientes.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { RdPhonePipe } from '../../../shared/ui/pipes/rd-phone.pipe';
import { Cliente, ClienteEstadoCuenta } from '../models/cliente.model';

const PAYMENT_METHODS = [
  { value: 1, label: 'Efectivo' },
  { value: 2, label: 'Tarjeta' },
  { value: 3, label: 'Transferencia' },
] as const;

@Component({
  selector: 'app-detalle-cliente',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, DatePipe, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe, RdPhonePipe],
  template: `
    <div class="space-y-5">

      <!-- Back + Header -->
      <div class="flex flex-col sm:flex-row sm:items-start gap-3">
        <button appBtn variant="ghost" size="sm" routerLink="/portal/clientes" class="self-start shrink-0">
          <iconify-icon icon="lucide:arrow-left" class="text-base"></iconify-icon>
          Clientes
        </button>
        @if (cliente(); as c) {
          <div class="flex-1 flex items-start justify-between gap-3">
            <div>
              <h2 class="text-2xl font-black text-base-content tracking-tight">{{ c.fullName }}</h2>
              <p class="text-sm text-base-content/50 mt-0.5">
                {{ c.phone | rdPhone }}
                @if (c.province) { · {{ c.province }} }
              </p>
            </div>
            <div class="flex items-center gap-2">
              <app-badge [variant]="c.isActive ? 'success' : 'ghost'">
                {{ c.isActive ? 'Activo' : 'Inactivo' }}
              </app-badge>
              <button appBtn size="sm" (click)="pagoModal.open()">
                <iconify-icon icon="lucide:circle-dollar-sign" class="text-base"></iconify-icon>
                Registrar pago
              </button>
            </div>
          </div>
        }
      </div>

      @if (loading()) {
        <div class="flex justify-center py-16"><app-spinner size="lg" /></div>
      } @else if (cliente(); as c) {

        <!-- Balance cards -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <app-card>
            <p class="text-[10px] text-base-content/40 uppercase tracking-widest font-black mb-1">Balance actual</p>
            <p [class]="c.balance < 0 ? 'text-3xl font-black text-secondary' : 'text-3xl font-black text-success'">
              {{ c.balance | rdCurrency }}
            </p>
            @if (c.balance < 0) {
              <p class="text-[11px] font-medium text-secondary/70 mt-1 italic">El cliente tiene deuda pendiente</p>
            }
          </app-card>
          <app-card>
            <p class="text-[10px] text-base-content/40 uppercase tracking-widest font-black mb-1">Límite de crédito</p>
            <p class="text-3xl font-black text-base-content">{{ c.creditLimit | rdCurrency }}</p>
          </app-card>
        </div>

        <!-- Estado de cuenta -->
        <app-card>
          <div class="flex items-center justify-between mb-3 px-1">
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest">Estado de cuenta</h3>
          </div>

          @if (estadoCuenta().length === 0) {
            <div class="flex flex-col items-center justify-center py-12 gap-3">
              <iconify-icon icon="lucide:file-text" class="text-5xl text-base-content/20"></iconify-icon>
              <p class="text-sm text-base-content/40">Sin movimientos registrados</p>
            </div>
          } @else {
            <app-table>
              <thead>
                <tr>
                  <th class="hidden sm:table-cell">Fecha</th>
                  <th>Tipo</th>
                  <th>Concepto</th>
                  <th class="text-right">Monto</th>
                </tr>
              </thead>
              <tbody>
                @for (t of estadoCuenta(); track t.transactionId) {
                  <tr class="hover">
                    <td class="hidden sm:table-cell text-sm text-base-content/50">
                      {{ t.date | date:'d MMM y, HH:mm' }}
                    </td>
                    <td>
                      <app-badge [variant]="t.type === 'Payment' ? 'success' : 'secondary'">
                        {{ t.type === 'Payment' ? 'Pago' : 'Cargo' }}
                      </app-badge>
                    </td>
                    <td class="text-sm text-base-content/70">{{ t.concept }}</td>
                    <td [class]="t.type === 'Payment' ? 'text-right font-semibold text-success' : 'text-right font-semibold text-secondary'">
                      {{ t.type === 'Payment' ? '+' : '-' }}{{ t.amount | rdCurrency }}
                    </td>
                  </tr>
                }
              </tbody>
            </app-table>
          }
        </app-card>
      }
    </div>

    <!-- Registrar Pago Modal -->
    <app-modal #pagoModal title="Registrar pago" (closed)="resetPagoForm()">
      <form [formGroup]="pagoForm" (ngSubmit)="onRegistrarPago(pagoModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="pag-amount">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Monto (RD$) *</span>
          </label>
          <input id="pag-amount" type="number" min="0.01" step="0.01" class="tc-input" formControlName="amount" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="pag-method">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Método de pago *</span>
          </label>
          <select id="pag-method" class="tc-input" formControlName="paymentMethodId">
            @for (m of paymentMethods; track m.value) {
              <option [value]="m.value">{{ m.label }}</option>
            }
          </select>
        </div>
        <div class="form-control">
          <label class="label pb-1" for="pag-concept">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Concepto *</span>
          </label>
          <input id="pag-concept" type="text" class="tc-input" formControlName="concept" placeholder="Abono fiado..." />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="pagoModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="pagoForm.invalid">
            Registrar pago
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class DetalleClientePage {
  private route = inject(ActivatedRoute);
  private svc = inject(ClientesService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  private clienteId = this.route.snapshot.paramMap.get('id') ?? '';

  loading = signal(true);
  saving = signal(false);
  cliente = signal<Cliente | null>(null);
  estadoCuenta = signal<ClienteEstadoCuenta[]>([]);

  readonly paymentMethods = PAYMENT_METHODS;

  pagoForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    paymentMethodId: [1, Validators.required],
    concept: ['', [Validators.required, Validators.minLength(3)]],
  });

  constructor() { this.loadData(); }

  loadData(): void {
    this.loading.set(true);
    this.svc.getCliente(this.clienteId).subscribe({
      next: (c) => this.cliente.set(c),
      error: () => this.toast.error('Error cargando cliente'),
    });
    this.svc.getEstadoCuenta(this.clienteId).subscribe({
      next: (items) => { this.estadoCuenta.set(items); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando estado de cuenta'); this.loading.set(false); },
    });
  }

  onRegistrarPago(modal: ModalComponent): void {
    if (this.pagoForm.invalid) return;
    this.saving.set(true);
    const { amount, paymentMethodId, concept } = this.pagoForm.getRawValue();
    this.svc.registrarPago(this.clienteId, amount, paymentMethodId, concept).subscribe({
      next: () => {
        this.toast.success('Pago registrado');
        modal.close();
        this.loadData();
        this.saving.set(false);
      },
      error: () => { this.toast.error('Error al registrar pago'); this.saving.set(false); },
    });
  }

  resetPagoForm(): void { this.pagoForm.reset({ amount: 0, paymentMethodId: 1, concept: '' }); }
}
