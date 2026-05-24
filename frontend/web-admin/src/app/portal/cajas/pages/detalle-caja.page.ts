import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CajasService } from '../services/cajas.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { EXPENSE_CATEGORIES, FondoMonetario, FondoTransaccion, TRANSACTION_TYPE_LABELS, TRANSACTION_TYPE_VARIANTS } from '../models/caja.model';

@Component({
  selector: 'app-detalle-caja',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, DatePipe, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe],
  template: `
    <div class="space-y-5">

      <!-- Back + Header -->
      <div class="flex flex-col sm:flex-row sm:items-start gap-3">
        <button appBtn variant="ghost" size="sm" routerLink="/portal/cajas" class="self-start shrink-0">
          <iconify-icon icon="lucide:arrow-left" class="text-base"></iconify-icon>
          Cajas
        </button>
        @if (fondo(); as f) {
          <div class="flex-1 flex items-start justify-between gap-3">
            <div>
              <h2 class="text-2xl font-black text-base-content tracking-tight">{{ f.name }}</h2>
              <p class="text-3xl font-black text-primary mt-1">{{ f.balance | rdCurrency }}</p>
            </div>
            <div class="flex gap-2">
              <button appBtn variant="outline" size="sm" (click)="gastoModal.open()">
                <iconify-icon icon="lucide:minus-circle" class="text-base"></iconify-icon>
                Gasto
              </button>
              <button appBtn size="sm" (click)="depositoModal.open()">
                <iconify-icon icon="lucide:plus-circle" class="text-base"></iconify-icon>
                Depósito
              </button>
            </div>
          </div>
        }
      </div>

      @if (loading()) {
        <div class="flex justify-center py-16"><app-spinner size="lg" /></div>
      } @else {

        <!-- Transacciones -->
        <app-card>
          <div class="flex items-center justify-between mb-3 px-1">
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest">Movimientos</h3>
          </div>

          @if (transacciones().length === 0) {
            <div class="flex flex-col items-center justify-center py-12 gap-3">
              <iconify-icon icon="lucide:receipt" class="text-5xl text-base-content/20"></iconify-icon>
              <p class="text-sm text-base-content/40">Sin movimientos registrados</p>
            </div>
          } @else {
            <app-table>
              <thead>
                <tr>
                  <th>Tipo</th>
                  <th>Descripción</th>
                  <th class="hidden sm:table-cell">Fecha</th>
                  <th class="text-right">Monto</th>
                </tr>
              </thead>
              <tbody>
                @for (t of transacciones(); track t.id) {
                  <tr class="hover">
                    <td>
                      <app-badge [variant]="getTypeVariant(t.type)">
                        {{ getTypeLabel(t.type) }}
                      </app-badge>
                    </td>
                    <td class="text-sm text-base-content/70">{{ t.description }}</td>
                    <td class="hidden sm:table-cell text-sm text-base-content/50">
                      {{ t.createdAt | date:'d MMM y, HH:mm' }}
                    </td>
                    <td [class]="t.type === 1 ? 'text-right font-semibold text-success' : 'text-right font-semibold text-secondary'">
                      {{ t.type === 1 ? '+' : '-' }}{{ t.amount | rdCurrency }}
                    </td>
                  </tr>
                }
              </tbody>
            </app-table>

            @if (totalPages() > 1) {
              <div class="flex items-center justify-between px-4 py-3 border-t border-base-300">
                <p class="text-sm text-base-content/50">{{ totalCount() }} movimientos</p>
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
      }
    </div>

    <!-- Depósito Modal -->
    <app-modal #depositoModal title="Registrar depósito" (closed)="resetDepositoForm()">
      <form [formGroup]="depositoForm" (ngSubmit)="onDepositar(depositoModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="dep-amount">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Monto (RD$) *</span>
          </label>
          <input id="dep-amount" type="number" min="0.01" step="0.01" class="tc-input" formControlName="amount" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="dep-desc">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Descripción *</span>
          </label>
          <input id="dep-desc" type="text" class="tc-input" formControlName="description" placeholder="Ingreso del día..." />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="depositoModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="depositoForm.invalid">
            Registrar
          </button>
        </div>
      </form>
    </app-modal>

    <!-- Gasto Modal -->
    <app-modal #gastoModal title="Registrar gasto" (closed)="resetGastoForm()">
      <form [formGroup]="gastoForm" (ngSubmit)="onRegistrarGasto(gastoModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="gas-amount">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Monto (RD$) *</span>
          </label>
          <input id="gas-amount" type="number" min="0.01" step="0.01" class="tc-input" formControlName="amount" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="gas-cat">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Categoría *</span>
          </label>
          <select id="gas-cat" class="tc-input" formControlName="category">
            @for (cat of expenseCategories; track cat.value) {
              <option [value]="cat.value">{{ cat.label }}</option>
            }
          </select>
        </div>
        <div class="form-control">
          <label class="label pb-1" for="gas-desc">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Descripción *</span>
          </label>
          <input id="gas-desc" type="text" class="tc-input" formControlName="description" placeholder="Detalle del gasto..." />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="gastoModal.close()">Cancelar</button>
          <button appBtn variant="secondary" type="submit" [loading]="saving()" [disabled]="gastoForm.invalid">
            Registrar gasto
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class DetalleCajaPage {
  private route = inject(ActivatedRoute);
  private svc = inject(CajasService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  private fondoId = this.route.snapshot.paramMap.get('id') ?? '';

  loading = signal(true);
  saving = signal(false);
  fondo = signal<FondoMonetario | null>(null);
  transacciones = signal<FondoTransaccion[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  page = signal(1);

  readonly expenseCategories = EXPENSE_CATEGORIES;

  depositoForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    description: ['', [Validators.required, Validators.minLength(3)]],
  });

  gastoForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    category: ['Other', Validators.required],
    description: ['', [Validators.required, Validators.minLength(3)]],
  });

  constructor() { this.loadData(); }

  loadData(): void {
    this.loading.set(true);
    this.svc.getFondo(this.fondoId).subscribe({
      next: (f: any) => this.fondo.set(f),
      error: () => this.toast.error('Error cargando fondo'),
    });
    this.loadTransacciones();
  }

  loadTransacciones(): void {
    this.svc.getTransacciones(this.fondoId, this.page(), 20).subscribe({
      next: (res: any) => {
        this.transacciones.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => { this.toast.error('Error cargando transacciones'); this.loading.set(false); },
    });
  }

  changePage(p: number): void { this.page.set(p); this.loadTransacciones(); }

  getTypeLabel(type: number): string { return TRANSACTION_TYPE_LABELS[type] ?? 'Otro'; }
  getTypeVariant(type: number): 'success' | 'secondary' | 'neutral' {
    return (TRANSACTION_TYPE_VARIANTS[type] as 'success' | 'secondary' | 'neutral') ?? 'neutral';
  }

  onDepositar(modal: ModalComponent): void {
    if (this.depositoForm.invalid) return;
    this.saving.set(true);
    const val = this.depositoForm.getRawValue();
    this.svc.depositar(this.fondoId, val.amount, val.description).subscribe({
      next: () => { this.toast.success('Depósito registrado'); modal.close(); this.loadData(); this.saving.set(false); },
      error: () => { this.toast.error('Error al depositar'); this.saving.set(false); },
    });
  }

  onRegistrarGasto(modal: ModalComponent): void {
    if (this.gastoForm.invalid) return;
    this.saving.set(true);
    const val = this.gastoForm.getRawValue();
    this.svc.registrarGasto(this.fondoId, val.amount, val.category, val.description).subscribe({
      next: () => { this.toast.success('Gasto registrado'); modal.close(); this.loadData(); this.saving.set(false); },
      error: () => { this.toast.error('Error al registrar gasto'); this.saving.set(false); },
    });
  }

  resetDepositoForm(): void { this.depositoForm.reset({ amount: 0, description: '' }); }
  resetGastoForm(): void { this.gastoForm.reset({ amount: 0, category: 'Other', description: '' }); }
}
