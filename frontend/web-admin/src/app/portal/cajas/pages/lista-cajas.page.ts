import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CajasService } from '../services/cajas.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { FondoMonetario } from '../models/caja.model';

@Component({
  selector: 'app-lista-cajas',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Cajas</h2>
        <button appBtn size="sm" (click)="fondoModal.open()">
          <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
          Nuevo fondo
        </button>
      </div>

      <!-- Fondos -->
      @if (loading()) {
        <div class="flex justify-center py-16">
          <app-spinner size="lg" />
        </div>
      } @else if (fondos().length === 0) {
        <div class="flex flex-col items-center justify-center py-16 gap-4">
          <iconify-icon icon="lucide:wallet" class="text-6xl text-base-content/20"></iconify-icon>
          <div class="text-center">
            <p class="font-medium text-base-content/60">No hay fondos monetarios</p>
            <p class="text-sm text-base-content/40 mt-1">Crea un fondo para registrar depósitos y gastos</p>
          </div>
          <button appBtn size="sm" (click)="fondoModal.open()">
            <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
            Crear fondo
          </button>
        </div>
      } @else {
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          @for (f of fondos(); track f.id) {
            <a
              [routerLink]="[f.id]"
              class="tc-card block p-5 hover:border-primary/40 hover:bg-primary/5 transition-colors shadow-sm"
            >
              <div class="flex items-start justify-between">
                <div class="w-10 h-10 bg-primary/10 flex items-center justify-center rounded-lg shrink-0">
                  <iconify-icon icon="lucide:wallet" class="text-primary text-xl"></iconify-icon>
                </div>
                @if (!f.isActive) {
                  <app-badge variant="ghost">Inactivo</app-badge>
                }
              </div>
              <div class="mt-3">
                <p class="font-bold text-base-content leading-tight">{{ f.name }}</p>
                <p class="text-2xl font-black text-primary mt-1">{{ f.balance | rdCurrency }}</p>
                <p class="text-xs text-base-content/40 mt-1 uppercase tracking-widest font-black">Balance disponible</p>
              </div>
            </a>
          }
        </div>
      }
    </div>

    <!-- Nuevo Fondo Modal -->
    <app-modal #fondoModal title="Nuevo fondo monetario" (closed)="resetForm()">
      <form [formGroup]="fondoForm" (ngSubmit)="onCreateFondo(fondoModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="fondo-name">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Nombre *</span>
          </label>
          <input id="fondo-name" type="text" class="tc-input" formControlName="name" placeholder="Caja principal, Caja 2..." />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="fondo-deposit">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Depósito inicial (RD$) *</span>
          </label>
          <input id="fondo-deposit" type="number" min="0" step="0.01" class="tc-input" formControlName="initialDeposit" />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="fondoModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="fondoForm.invalid">
            Crear fondo
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class ListaCajasPage {
  private svc = inject(CajasService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  fondos = signal<FondoMonetario[]>([]);

  fondoForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    initialDeposit: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() { this.loadFondos(); }

  loadFondos(): void {
    this.loading.set(true);
    this.svc.getFondos().subscribe({
      next: (fondos: any) => { this.fondos.set(fondos); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando fondos'); this.loading.set(false); },
    });
  }

  onCreateFondo(modal: ModalComponent): void {
    if (this.fondoForm.invalid) return;
    this.saving.set(true);
    const { name, initialDeposit } = this.fondoForm.getRawValue();
    this.svc.createFondo(name, initialDeposit).subscribe({
      next: () => {
        this.toast.success('Fondo creado');
        modal.close();
        this.loadFondos();
        this.saving.set(false);
      },
      error: () => { this.toast.error('Error al crear fondo'); this.saving.set(false); },
    });
  }

  resetForm(): void { this.fondoForm.reset({ name: '', initialDeposit: 0 }); }
}
