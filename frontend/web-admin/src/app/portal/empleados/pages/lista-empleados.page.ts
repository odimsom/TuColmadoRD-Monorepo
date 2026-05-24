import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmpleadosService } from '../services/empleados.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { Empleado, ROLE_LABELS, ROLE_VARIANTS } from '../models/empleado.model';

const ROLES_DISPONIBLES = [
  { value: 'Admin', label: 'Administrador' },
  { value: 'Seller', label: 'Vendedor' },
  { value: 'Cashier', label: 'Cajero' },
  { value: 'Delivery', label: 'Repartidor' },
] as const;

@Component({
  selector: 'app-lista-empleados',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Empleados</h2>
        <button appBtn size="sm" (click)="empleadoModal.open()">
          <iconify-icon icon="lucide:user-plus" class="text-base"></iconify-icon>
          Nuevo empleado
        </button>
      </div>

      <!-- Table -->
      <app-card>
        @if (loading()) {
          <div class="flex justify-center py-16">
            <app-spinner size="lg" />
          </div>
        } @else if (empleados().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 gap-4">
            <iconify-icon icon="lucide:users" class="text-6xl text-base-content/20"></iconify-icon>
            <div class="text-center">
              <p class="font-medium text-base-content/60">No hay empleados registrados</p>
              <p class="text-sm text-base-content/40 mt-1">Agrega empleados para gestionar accesos</p>
            </div>
          </div>
        } @else {
          <app-table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Correo</th>
                <th>Rol</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              @for (e of empleados(); track e.id) {
                <tr class="hover">
                  <td class="font-medium text-base-content">
                    {{ e.firstName }} {{ e.lastName }}
                  </td>
                  <td class="text-sm text-base-content/60">{{ e.email }}</td>
                  <td>
                    <app-badge [variant]="getRoleVariant(e.role)">
                      {{ getRoleLabel(e.role) }}
                    </app-badge>
                  </td>
                  <td>
                    <app-badge [variant]="e.isActive ? 'success' : 'ghost'" size="xs">
                      {{ e.isActive ? 'Activo' : 'Inactivo' }}
                    </app-badge>
                  </td>
                </tr>
              }
            </tbody>
          </app-table>
        }
      </app-card>
    </div>

    <!-- Nuevo Empleado Modal -->
    <app-modal #empleadoModal title="Nuevo empleado" (closed)="resetForm()">
      <form [formGroup]="empleadoForm" (ngSubmit)="onCreateEmpleado(empleadoModal)" class="space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div class="form-control">
            <label class="label pb-1" for="emp-first">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Nombre *</span>
            </label>
            <input id="emp-first" type="text" class="tc-input" formControlName="firstName" placeholder="Juan" />
          </div>
          <div class="form-control">
            <label class="label pb-1" for="emp-last">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Apellido *</span>
            </label>
            <input id="emp-last" type="text" class="tc-input" formControlName="lastName" placeholder="Pérez" />
          </div>
        </div>
        <div class="form-control">
          <label class="label pb-1" for="emp-email">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Correo electrónico *</span>
          </label>
          <input id="emp-email" type="email" class="tc-input" formControlName="email" placeholder="empleado@tucol..." />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="emp-pass">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Contraseña *</span>
          </label>
          <input id="emp-pass" type="password" class="tc-input" formControlName="password" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="emp-role">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Rol *</span>
          </label>
          <select id="emp-role" class="tc-input" formControlName="role">
            @for (r of roles; track r.value) {
              <option [value]="r.value">{{ r.label }}</option>
            }
          </select>
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="empleadoModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="empleadoForm.invalid">
            Crear empleado
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class ListaEmpleadosPage {
  private svc = inject(EmpleadosService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  empleados = signal<Empleado[]>([]);

  readonly roles = ROLES_DISPONIBLES;

  empleadoForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Seller', Validators.required],
  });

  constructor() { this.loadEmpleados(); }

  loadEmpleados(): void {
    this.loading.set(true);
    this.svc.getEmpleados().subscribe({
      next: (emps) => { this.empleados.set(emps); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando empleados'); this.loading.set(false); },
    });
  }

  getRoleLabel(role: string): string { return ROLE_LABELS[role] ?? role; }

  getRoleVariant(role: string): 'primary' | 'secondary' | 'info' | 'accent' | 'neutral' {
    return (ROLE_VARIANTS[role] as 'primary' | 'secondary' | 'info' | 'accent' | 'neutral') ?? 'neutral';
  }

  onCreateEmpleado(modal: ModalComponent): void {
    if (this.empleadoForm.invalid) return;
    this.saving.set(true);
    this.svc.createEmpleado(this.empleadoForm.getRawValue()).subscribe({
      next: () => {
        this.toast.success('Empleado creado');
        modal.close();
        this.loadEmpleados();
        this.saving.set(false);
      },
      error: () => { this.toast.error('Error al crear empleado'); this.saving.set(false); },
    });
  }

  resetForm(): void {
    this.empleadoForm.reset({ firstName: '', lastName: '', email: '', password: '', role: 'Seller' });
  }
}
