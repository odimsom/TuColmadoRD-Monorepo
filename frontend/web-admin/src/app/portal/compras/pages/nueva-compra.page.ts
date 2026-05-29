import {
  Component, ChangeDetectionStrategy, inject, signal, computed, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ComprasService } from '../services/compras.service';
import { InventarioService } from '../../inventario/services/inventario.service';
import { CajasService } from '../../cajas/services/cajas.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../../../shared/ui/pipes/rd-currency.pipe';

interface PresentacionDisponible {
  id: string;
  displayName: string;
  productName: string;
  measureUnit: number;
  nominalCapacity: number;
}

@Component({
  selector: 'app-nueva-compra',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, CardComponent, SpinnerComponent, BtnComponent, RdCurrencyPipe],
  template: `
    <div class="space-y-5">

      <!-- Back + Header -->
      <div class="flex items-center gap-3">
        <button appBtn variant="ghost" size="sm" (click)="router.navigate(['/portal/compras'])">
          <iconify-icon icon="lucide:arrow-left" class="text-base"></iconify-icon>
          Compras
        </button>
        <h2 class="text-2xl font-black text-base-content tracking-tight">Nueva entrada de stock</h2>
      </div>

      @if (loadingData()) {
        <div class="flex justify-center py-16">
          <app-spinner size="lg" />
        </div>
      } @else {

        <form [formGroup]="compraForm" (ngSubmit)="onSubmit()" class="space-y-5">

          <!-- Información general -->
          <app-card>
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest mb-4">Información general</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div class="form-control">
                <label class="label pb-1" for="fecha">
                  <span class="text-xs font-bold text-base-content uppercase tracking-wider">Fecha de compra *</span>
                </label>
                <input id="fecha" type="datetime-local" class="tc-input" formControlName="purchasedAt" />
              </div>
              <div class="form-control">
                <label class="label pb-1" for="proveedor">
                  <span class="text-xs font-bold text-base-content uppercase tracking-wider">Proveedor</span>
                  <span class="text-[10px] text-base-content/40 uppercase font-bold">Opcional</span>
                </label>
                <input id="proveedor" type="text" class="tc-input" formControlName="supplierName" placeholder="Nombre del proveedor..." />
              </div>
            </div>
            <div class="form-control mt-4">
              <label class="label pb-1" for="notas">
                <span class="text-xs font-bold text-base-content uppercase tracking-wider">Notas</span>
                <span class="text-[10px] text-base-content/40 uppercase font-bold">Opcional</span>
              </label>
              <textarea id="notas" rows="2" class="tc-input" formControlName="notes" placeholder="Observaciones..."></textarea>
            </div>
          </app-card>

          <!-- Pago desde fondo monetario -->
          <app-card>
            <h3 class="font-bold text-base-content uppercase text-xs tracking-widest mb-4">Pago desde caja (opcional)</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div class="form-control">
                <label class="label pb-1" for="fondo">
                  <span class="text-xs font-bold text-base-content uppercase tracking-wider">Fondo monetario</span>
                </label>
                <select id="fondo" class="tc-input" formControlName="fundId">
                  <option value="">No descontar de caja</option>
                  @for (f of fondos(); track f.id) {
                    <option [value]="f.id">{{ f.name }} ({{ f.balance | rdCurrency }})</option>
                  }
                </select>
              </div>
              @if (compraForm.value.fundId) {
                <div class="form-control">
                  <label class="label pb-1" for="justificacion">
                    <span class="text-xs font-bold text-base-content uppercase tracking-wider">Justificación del gasto *</span>
                  </label>
                  <input id="justificacion" type="text" class="tc-input" formControlName="fundExpenseJustification" placeholder="Compra de inventario..." />
                </div>
              }
            </div>
          </app-card>

          <!-- Líneas de compra -->
          <app-card>
            <div class="flex items-center justify-between mb-4">
              <h3 class="font-bold text-base-content uppercase text-xs tracking-widest">Líneas de compra</h3>
              <button type="button" appBtn variant="outline" size="sm" (click)="agregarLinea()">
                <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
                Agregar línea
              </button>
            </div>

            <div formArrayName="lines" class="space-y-3">
              @for (lineControl of lines.controls; track $index) {
                <div [formGroupName]="$index" class="p-4 bg-base-200 rounded-lg space-y-3">
                  <div class="flex items-center justify-between">
                    <span class="text-xs font-bold text-base-content uppercase tracking-wider">Línea {{ $index + 1 }}</span>
                    <button type="button" class="tc-btn tc-btn-ghost tc-btn-xs" (click)="eliminarLinea($index)">
                      <iconify-icon icon="lucide:trash-2" class="text-base"></iconify-icon>
                    </button>
                  </div>

                  <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                    <div class="form-control col-span-full">
                      <label class="label pb-1">
                        <span class="text-xs font-bold text-base-content uppercase tracking-wider">Presentación *</span>
                      </label>
                      <select class="tc-input text-sm" formControlName="presentationId">
                        <option value="">Seleccionar...</option>
                        @for (p of presentaciones(); track p.id) {
                          <option [value]="p.id">{{ p.productName }} - {{ p.displayName }}</option>
                        }
                      </select>
                    </div>
                    <div class="form-control">
                      <label class="label pb-1">
                        <span class="text-xs font-bold text-base-content uppercase tracking-wider">Contenedores *</span>
                      </label>
                      <input type="number" min="1" class="tc-input" formControlName="containerCount" />
                    </div>
                    <div class="form-control">
                      <label class="label pb-1">
                        <span class="text-xs font-bold text-base-content uppercase tracking-wider">Unidades/contenedor *</span>
                      </label>
                      <input type="number" min="1" class="tc-input" formControlName="unitsPerContainer" />
                    </div>
                    <div class="form-control">
                      <label class="label pb-1">
                        <span class="text-xs font-bold text-base-content uppercase tracking-wider">Tamaño nominal/unidad *</span>
                      </label>
                      <input type="number" min="0" step="0.01" class="tc-input" formControlName="nominalSizePerUnit" />
                    </div>
                    <div class="form-control">
                      <label class="label pb-1">
                        <span class="text-xs font-bold text-base-content uppercase tracking-wider">Costo/unidad (RD$) *</span>
                      </label>
                      <input type="number" min="0" step="0.01" class="tc-input" formControlName="costPerUnit" />
                    </div>
                    <div class="form-control">
                      <label class="label pb-1">
                        <span class="text-xs font-bold text-base-content uppercase tracking-wider">Subtotal</span>
                      </label>
                      <div class="tc-input bg-base-100 flex items-center font-black text-primary">
                        {{ calcularSubtotal($index) | rdCurrency }}
                      </div>
                    </div>
                  </div>
                </div>
              }

              @if (lines.length === 0) {
                <div class="flex flex-col items-center justify-center py-12 gap-3">
                  <iconify-icon icon="lucide:inbox" class="text-5xl text-base-content/20"></iconify-icon>
                  <p class="text-sm text-base-content/40">Agrega líneas de compra para continuar</p>
                  <button type="button" appBtn variant="outline" size="sm" (click)="agregarLinea()">
                    <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
                    Agregar línea
                  </button>
                </div>
              }
            </div>

            @if (lines.length > 0) {
              <div class="mt-6 pt-4 border-t border-base-300 flex justify-between items-center">
                <span class="text-sm font-bold text-base-content uppercase tracking-wider">Total</span>
                <span class="text-2xl font-black text-primary">{{ totalCompra() | rdCurrency }}</span>
              </div>
            }
          </app-card>

          <!-- Acciones -->
          <div class="flex justify-end gap-3">
            <button type="button" class="tc-btn tc-btn-ghost" (click)="router.navigate(['/portal/compras'])">
              Cancelar
            </button>
            <button appBtn type="submit" [loading]="saving()" [disabled]="compraForm.invalid || lines.length === 0">
              <iconify-icon icon="lucide:check" class="text-base"></iconify-icon>
              Confirmar entrada de stock
            </button>
          </div>
        </form>
      }
    </div>
  `,
})
export class NuevaCompraPage {
  readonly router = inject(Router);
  private comprasSvc = inject(ComprasService);
  private inventarioSvc = inject(InventarioService);
  private cajasSvc = inject(CajasService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  loadingData = signal(true);
  saving = signal(false);
  presentaciones = signal<PresentacionDisponible[]>([]);
  fondos = signal<any[]>([]);

  compraForm = this.fb.nonNullable.group({
    purchasedAt: [this.getDefaultDateTime(), Validators.required],
    supplierName: [''],
    notes: [''],
    fundId: [''],
    fundExpenseJustification: [''],
    lines: this.fb.array([]),
  });

  get lines(): FormArray {
    return this.compraForm.get('lines') as FormArray;
  }

  totalCompra = computed(() => {
    let total = 0;
    for (let i = 0; i < this.lines.length; i++) {
      total += this.calcularSubtotal(i);
    }
    return total;
  });

  constructor() {
    this.loadData();
  }

  loadData(): void {
    this.loadingData.set(true);
    Promise.all([
      this.inventarioSvc.getCatalogo().toPromise(),
      this.cajasSvc.getFondos().toPromise(),
    ]).then(([catalogo, fondos]) => {
      const presen: PresentacionDisponible[] = [];
      catalogo?.forEach((prod: any) => {
        prod.presentations?.forEach((p: any) => {
          presen.push({
            id: p.id,
            displayName: p.displayName,
            productName: prod.name,
            measureUnit: p.measureUnit,
            nominalCapacity: p.nominalCapacity ?? 0,
          });
        });
      });
      this.presentaciones.set(presen);
      this.fondos.set(fondos ?? []);
      this.loadingData.set(false);
    }).catch(() => {
      this.toast.error('Error cargando datos');
      this.loadingData.set(false);
    });
  }

  getDefaultDateTime(): string {
    const now = new Date();
    const tzOffset = now.getTimezoneOffset() * 60000;
    const localISOTime = new Date(now.getTime() - tzOffset).toISOString().slice(0, 16);
    return localISOTime;
  }

  agregarLinea(): void {
    const lineForm = this.fb.nonNullable.group({
      presentationId: ['', Validators.required],
      containerCount: [1, [Validators.required, Validators.min(1)]],
      unitsPerContainer: [1, [Validators.required, Validators.min(1)]],
      nominalSizePerUnit: [0, [Validators.required, Validators.min(0)]],
      costPerUnit: [0, [Validators.required, Validators.min(0)]],
    });
    this.lines.push(lineForm);
  }

  eliminarLinea(index: number): void {
    this.lines.removeAt(index);
  }

  calcularSubtotal(index: number): number {
    const line = this.lines.at(index).value;
    return (line.containerCount ?? 0) * (line.unitsPerContainer ?? 0) * (line.costPerUnit ?? 0);
  }

  onSubmit(): void {
    if (this.compraForm.invalid || this.lines.length === 0) return;

    const val = this.compraForm.getRawValue();

    if (val.fundId && !val.fundExpenseJustification?.trim()) {
      this.toast.error('Debe ingresar una justificación si desea descontar de caja');
      return;
    }

    this.saving.set(true);

    const payload = {
      purchasedAt: new Date(val.purchasedAt).toISOString(),
      supplierName: val.supplierName || null,
      notes: val.notes || null,
      fundId: val.fundId || null,
      fundExpenseJustification: val.fundExpenseJustification || null,
      lines: val.lines.map((l: any) => ({
        presentationId: l.presentationId,
        containerCount: l.containerCount,
        unitsPerContainer: l.unitsPerContainer,
        nominalSizePerUnit: l.nominalSizePerUnit,
        costPerUnit: l.costPerUnit,
      })),
    };

    this.comprasSvc.createStockEntry(payload).subscribe({
      next: () => {
        this.toast.success('Entrada de stock confirmada');
        this.router.navigate(['/portal/compras']);
      },
      error: () => {
        this.toast.error('Error al confirmar entrada de stock');
        this.saving.set(false);
      },
    });
  }
}
