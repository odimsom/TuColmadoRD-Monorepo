import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventarioService } from '../services/inventario.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';
import { RdItbisPipe } from '../../../shared/ui/pipes/rd-itbis.pipe';
import {
  Presentacion, Producto,
  ITBIS_OPTIONS, MEASURE_UNIT_LABELS, MEASURE_UNIT_OPTIONS,
  PRESENTATION_TYPE_OPTIONS, SELL_MODE_LABELS, SELL_MODE_OPTIONS,
} from '../models/producto.model';

@Component({
  selector: 'app-detalle-producto',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, DatePipe, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe, RdItbisPipe],
  template: `
    <div class="space-y-5">

      <!-- Back + Header -->
      <div class="flex flex-col sm:flex-row sm:items-start gap-3">
        <button appBtn variant="ghost" size="sm" routerLink="/portal/inventario" class="self-start shrink-0">
          <iconify-icon icon="lucide:arrow-left" class="text-base"></iconify-icon>
          Inventario
        </button>
        @if (producto(); as p) {
          <div class="flex-1 flex items-start justify-between gap-3">
            <div>
              <h2 class="text-2xl font-black text-base-content tracking-tight">{{ p.name }}</h2>
              <p class="text-sm text-base-content/50 mt-0.5">{{ p.categoryName }}</p>
            </div>
            @if (p.isActive) {
              <button appBtn variant="error" size="sm" [loading]="saving()" (click)="onDeactivateProducto(p.id)">
                <iconify-icon icon="lucide:trash-2" class="text-base"></iconify-icon>
                Desactivar
              </button>
            }
          </div>
        }
      </div>

      <!-- Loading -->
      @if (loading()) {
        <div class="flex justify-center py-16">
          <app-spinner size="lg" />
        </div>
      } @else if (producto(); as p) {

        <!-- Info Card -->
        <app-card [compact]="true">
          <dl class="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <div>
              <dt class="text-xs font-bold text-base-content/40 uppercase tracking-widest">ITBIS</dt>
              <dd class="mt-1 font-semibold text-base-content">{{ p.itbisRate | rdItbis }}</dd>
            </div>
            <div>
              <dt class="text-xs font-bold text-base-content/40 uppercase tracking-widest">Estado</dt>
              <dd class="mt-1">
                <app-badge [variant]="p.isActive ? 'success' : 'ghost'">
                  {{ p.isActive ? 'Activo' : 'Inactivo' }}
                </app-badge>
              </dd>
            </div>
            <div>
              <dt class="text-xs font-bold text-base-content/40 uppercase tracking-widest">Presentaciones</dt>
              <dd class="mt-1 font-semibold text-base-content">{{ p.presentations.length }}</dd>
            </div>
          </dl>
        </app-card>

        <!-- Presentations -->
        <div>
          <div class="flex items-center justify-between mb-3 px-1">
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest">Presentaciones</h3>
            <button appBtn size="sm" (click)="presentacionModal.open()">
              <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
              Nueva presentación
            </button>
          </div>

          <app-card>
            @if (p.presentations.length === 0) {
              <div class="flex flex-col items-center justify-center py-10 gap-3">
                <iconify-icon icon="lucide:boxes" class="text-5xl text-base-content/20"></iconify-icon>
                <p class="text-sm text-base-content/40">Sin presentaciones. Agrega la primera.</p>
              </div>
            } @else {
              <app-table>
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th class="hidden sm:table-cell">Modo venta</th>
                    <th>Precio venta</th>
                    <th class="hidden md:table-cell">Costo</th>
                    <th>Estado</th>
                    <th class="w-16"></th>
                  </tr>
                </thead>
                <tbody>
                  @for (pres of p.presentations; track pres.id) {
                    <tr class="hover">
                      <td class="font-medium text-base-content">{{ pres.displayName }}</td>
                      <td class="hidden sm:table-cell text-sm text-base-content/60">{{ sellModeLabels[pres.sellMode] }}</td>
                      <td class="font-semibold text-base-content">{{ pres.salePrice | rdCurrency }}</td>
                      <td class="hidden md:table-cell text-sm text-base-content/60">{{ pres.costPrice | rdCurrency }}</td>
                      <td>
                        <app-badge [variant]="pres.isActive ? 'success' : 'ghost'">
                          {{ pres.isActive ? 'Activa' : 'Inactiva' }}
                        </app-badge>
                      </td>
                      <td>
                        <div class="flex gap-1">
                          @if (pres.isActive) {
                            <button
                              appBtn variant="ghost" size="sm" [square]="true"
                              aria-label="Editar precio"
                              (click)="openEditPrice(pres, editPriceModal)"
                            >
                              <iconify-icon icon="lucide:pencil" class="text-sm"></iconify-icon>
                            </button>
                            <button
                              appBtn variant="ghost" size="sm" [square]="true"
                              class="text-secondary"
                              aria-label="Desactivar"
                              (click)="onDeactivatePresentacion(pres.id)"
                            >
                              <iconify-icon icon="lucide:x" class="text-sm"></iconify-icon>
                            </button>
                          }
                        </div>
                      </td>
                    </tr>
                  }
                </tbody>
              </app-table>
            }
          </app-card>
        </div>
      }
    </div>

    <!-- Nueva Presentación Modal -->
    <app-modal #presentacionModal title="Nueva presentación" (closed)="resetPresentacionForm()">
      <form [formGroup]="presentacionForm" (ngSubmit)="onAddPresentacion(presentacionModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="pres-name">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Nombre *</span>
          </label>
          <input id="pres-name" type="text" class="tc-input" formControlName="displayName" placeholder="Lb 5, Funda 500g..." />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="form-control">
            <label class="label pb-1" for="pres-type">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Tipo</span>
            </label>
            <select id="pres-type" class="tc-input" formControlName="presentationType">
              @for (opt of presentationTypeOpts; track opt.value) {
                <option [value]="opt.value">{{ opt.label }}</option>
              }
            </select>
          </div>
          <div class="form-control">
            <label class="label pb-1" for="pres-mode">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Modo venta</span>
            </label>
            <select id="pres-mode" class="tc-input" formControlName="sellMode">
              @for (opt of sellModeOpts; track opt.value) {
                <option [value]="opt.value">{{ opt.label }}</option>
              }
            </select>
          </div>
        </div>
        <div class="form-control">
          <label class="label pb-1" for="pres-unit">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Unidad de medida</span>
          </label>
          <select id="pres-unit" class="tc-input" formControlName="measureUnit">
            @for (opt of measureUnitOpts; track opt.value) {
              <option [value]="opt.value">{{ opt.label }}</option>
            }
          </select>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="form-control">
            <label class="label pb-1" for="pres-sale">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Precio venta (RD$) *</span>
            </label>
            <input id="pres-sale" type="number" min="0" step="0.01" class="tc-input" formControlName="salePrice" />
          </div>
          <div class="form-control">
            <label class="label pb-1" for="pres-cost">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Costo (RD$) *</span>
            </label>
            <input id="pres-cost" type="number" min="0" step="0.01" class="tc-input" formControlName="costPrice" />
          </div>
        </div>
        <div class="form-control">
          <label class="label pb-1" for="pres-brand">
            <span class="text-xs font-bold text-base-content uppercase tracking-wider">Marca (opcional)</span>
          </label>
          <input id="pres-brand" type="text" class="tc-input" formControlName="brand" placeholder="Marca del producto..." />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="presentacionModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="presentacionForm.invalid">
            Agregar
          </button>
        </div>
      </form>
    </app-modal>

    <!-- Editar Precio Modal -->
    <app-modal #editPriceModal title="Editar precio">
      <form [formGroup]="editPriceForm" (ngSubmit)="onUpdatePrice(editPriceModal)" class="space-y-4">
        @if (editingPresentacion(); as ep) {
          <p class="text-sm text-base-content/60 px-1">{{ ep.displayName }}</p>
        }
        <div class="grid grid-cols-2 gap-4">
          <div class="form-control">
            <label class="label pb-1" for="edit-sale">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Precio venta (RD$)</span>
            </label>
            <input id="edit-sale" type="number" min="0" step="0.01" class="tc-input" formControlName="salePrice" />
          </div>
          <div class="form-control">
            <label class="label pb-1" for="edit-cost">
              <span class="text-xs font-bold text-base-content uppercase tracking-wider">Costo (RD$)</span>
            </label>
            <input id="edit-cost" type="number" min="0" step="0.01" class="tc-input" formControlName="costPrice" />
          </div>
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="editPriceModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="editPriceForm.invalid">
            Actualizar
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class DetalleProductoPage {
  private route = inject(ActivatedRoute);
  private svc = inject(InventarioService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  private productId = this.route.snapshot.paramMap.get('id') ?? '';

  loading = signal(true);
  saving = signal(false);
  producto = signal<Producto | null>(null);
  editingPresentacion = signal<Presentacion | null>(null);

  presentacionForm = this.fb.nonNullable.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    presentationType: [2],
    sellMode: [1],
    measureUnit: [1],
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    costPrice: [0, [Validators.required, Validators.min(0)]],
    brand: [''],
  });

  editPriceForm = this.fb.nonNullable.group({
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    costPrice: [0, [Validators.required, Validators.min(0)]],
  });

  readonly itbisOpts = ITBIS_OPTIONS;
  readonly presentationTypeOpts = PRESENTATION_TYPE_OPTIONS;
  readonly sellModeOpts = SELL_MODE_OPTIONS;
  readonly measureUnitOpts = MEASURE_UNIT_OPTIONS;
  readonly sellModeLabels = SELL_MODE_LABELS;
  readonly measureUnitLabels = MEASURE_UNIT_LABELS;

  constructor() { this.loadProducto(); }

  loadProducto(): void {
    this.loading.set(true);
    this.svc.getProducto(this.productId).subscribe({
      next: (p) => { this.producto.set(p); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando producto'); this.loading.set(false); },
    });
  }

  onDeactivateProducto(id: string): void {
    this.saving.set(true);
    this.svc.deactivateProducto(id).subscribe({
      next: () => { this.toast.success('Producto desactivado'); this.loadProducto(); this.saving.set(false); },
      error: () => { this.toast.error('Error al desactivar producto'); this.saving.set(false); },
    });
  }

  onAddPresentacion(modal: ModalComponent): void {
    if (this.presentacionForm.invalid) return;
    this.saving.set(true);
    const val = this.presentacionForm.getRawValue();
    this.svc.addPresentacion(this.productId, {
      displayName: val.displayName,
      presentationType: val.presentationType,
      sellMode: val.sellMode,
      measureUnit: val.measureUnit,
      salePrice: val.salePrice,
      costPrice: val.costPrice,
      brand: val.brand || null,
    }).subscribe({
      next: () => { this.toast.success('Presentación agregada'); modal.close(); this.loadProducto(); this.saving.set(false); },
      error: () => { this.toast.error('Error al agregar presentación'); this.saving.set(false); },
    });
  }

  openEditPrice(pres: Presentacion, modal: ModalComponent): void {
    this.editingPresentacion.set(pres);
    this.editPriceForm.reset({ salePrice: pres.salePrice, costPrice: pres.costPrice });
    modal.open();
  }

  onUpdatePrice(modal: ModalComponent): void {
    const ep = this.editingPresentacion();
    if (this.editPriceForm.invalid || !ep) return;
    this.saving.set(true);
    const { salePrice, costPrice } = this.editPriceForm.getRawValue();
    this.svc.updatePresentacionPrice(ep.id, salePrice, costPrice).subscribe({
      next: () => { this.toast.success('Precio actualizado'); modal.close(); this.loadProducto(); this.saving.set(false); },
      error: () => { this.toast.error('Error al actualizar precio'); this.saving.set(false); },
    });
  }

  onDeactivatePresentacion(id: string): void {
    this.svc.deactivatePresentacion(id).subscribe({
      next: () => { this.toast.success('Presentación desactivada'); this.loadProducto(); },
      error: () => this.toast.error('Error al desactivar presentación'),
    });
  }

  resetPresentacionForm(): void {
    this.presentacionForm.reset({ displayName: '', presentationType: 2, sellMode: 1, measureUnit: 1, salePrice: 0, costPrice: 0, brand: '' });
  }
}
