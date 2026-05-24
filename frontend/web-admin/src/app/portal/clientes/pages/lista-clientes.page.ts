import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
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
import { Cliente } from '../models/cliente.model';

@Component({
  selector: 'app-lista-clientes',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe, RdPhonePipe],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Clientes</h2>
        <button appBtn size="sm" (click)="clienteModal.open()">
          <iconify-icon icon="lucide:user-plus" class="text-base"></iconify-icon>
          Nuevo cliente
        </button>
      </div>

      <!-- Table -->
      <app-card>
        @if (loading()) {
          <div class="flex justify-center py-16">
            <app-spinner size="lg" />
          </div>
        } @else if (clientes().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 gap-4">
            <iconify-icon icon="lucide:users" class="text-6xl text-base-content/20"></iconify-icon>
            <div class="text-center">
              <p class="font-medium text-base-content/60">No hay clientes registrados</p>
              <p class="text-sm text-base-content/40 mt-1">Registra clientes para gestionar fiados</p>
            </div>
            <button appBtn size="sm" (click)="clienteModal.open()">
              <iconify-icon icon="lucide:user-plus" class="text-base"></iconify-icon>
              Nuevo cliente
            </button>
          </div>
        } @else {
          <app-table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th class="hidden sm:table-cell">Teléfono</th>
                <th>Balance</th>
                <th class="hidden md:table-cell">Límite crédito</th>
                <th>Estado</th>
                <th class="w-8"></th>
              </tr>
            </thead>
            <tbody>
              @for (c of clientes(); track c.customerId) {
                <tr class="hover cursor-pointer" [routerLink]="[c.customerId]">
                  <td class="font-medium text-base-content">{{ c.fullName }}</td>
                  <td class="hidden sm:table-cell text-sm text-base-content/60">{{ c.phone | rdPhone }}</td>
                  <td [class]="c.balance < 0 ? 'font-semibold text-secondary' : 'font-semibold text-base-content'">
                    {{ c.balance | rdCurrency }}
                  </td>
                  <td class="hidden md:table-cell text-sm text-base-content/60">{{ c.creditLimit | rdCurrency }}</td>
                  <td>
                    <app-badge [variant]="c.isActive ? 'success' : 'ghost'">
                      {{ c.isActive ? 'Activo' : 'Inactivo' }}
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
              <p class="text-sm text-base-content/50">{{ totalCount() }} cliente{{ totalCount() !== 1 ? 's' : '' }}</p>
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

    <!-- Nuevo Cliente Modal -->
    <app-modal #clienteModal title="Nuevo cliente" (closed)="resetForm()">
      <form [formGroup]="clienteForm" (ngSubmit)="onCreateCliente(clienteModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="cli-name">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Nombre completo *</span>
          </label>
          <input id="cli-name" type="text" class="tc-input" formControlName="fullName" placeholder="Juan Pérez..." />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="cli-doc">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Cédula / Documento *</span>
          </label>
          <input id="cli-doc" type="text" class="tc-input" formControlName="documentId" placeholder="001-0000000-0" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="cli-phone">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Teléfono</span>
          </label>
          <input id="cli-phone" type="tel" class="tc-input" formControlName="phone" placeholder="809-000-0000" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="cli-credit">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Límite de crédito (RD$)</span>
          </label>
          <input id="cli-credit" type="number" min="0" step="0.01" class="tc-input" formControlName="creditLimit" />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="clienteModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="clienteForm.invalid">
            Crear cliente
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class ListaClientesPage {
  private svc = inject(ClientesService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  clientes = signal<Cliente[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  page = signal(1);

  clienteForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    documentId: ['', Validators.required],
    phone: [''],
    creditLimit: [0, [Validators.min(0)]],
  });

  constructor() { this.loadClientes(); }

  loadClientes(): void {
    this.loading.set(true);
    this.svc.getClientes(this.page(), 20).subscribe({
      next: (res: any) => {
        this.clientes.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => { this.toast.error('Error cargando clientes'); this.loading.set(false); },
    });
  }

  changePage(p: number): void { this.page.set(p); this.loadClientes(); }

  onCreateCliente(modal: ModalComponent): void {
    if (this.clienteForm.invalid) return;
    this.saving.set(true);
    this.svc.createCliente(this.clienteForm.getRawValue()).subscribe({
      next: () => { this.toast.success('Cliente creado'); modal.close(); this.loadClientes(); this.saving.set(false); },
      error: () => { this.toast.error('Error al crear cliente'); this.saving.set(false); },
    });
  }

  resetForm(): void {
    this.clienteForm.reset({ fullName: '', documentId: '', phone: '', creditLimit: 0 });
  }
}
