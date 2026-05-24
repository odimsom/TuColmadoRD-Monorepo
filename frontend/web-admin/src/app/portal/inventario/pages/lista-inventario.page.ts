import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InventarioService } from '../services/inventario.service';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { TableComponent } from '../../../shared/ui/table/table.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../../../shared/ui/badge/badge.component';
import { ModalComponent } from '../../../shared/ui/modal/modal.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { Categoria, ITBIS_OPTIONS, ProductoResumen } from '../models/producto.model';

import { RdItbisPipe } from '../../../shared/ui/pipes/rd-itbis.pipe';

@Component({
  selector: 'app-lista-inventario',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [RouterLink, ReactiveFormsModule, CardComponent, TableComponent, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdItbisPipe],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <h2 class="text-2xl font-black text-base-content tracking-tight">Inventario</h2>
        <div class="flex gap-2">
          <button appBtn variant="outline" size="sm" (click)="categoriaModal.open()">
            <iconify-icon icon="lucide:tag" class="text-base"></iconify-icon>
            Nueva categoría
          </button>
          <button appBtn size="sm" (click)="productoModal.open()">
            <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
            Nuevo producto
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1">
          <iconify-icon
            icon="lucide:search"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-base-content/40 pointer-events-none"
          ></iconify-icon>
          <input
            type="text"
            placeholder="Buscar producto..."
            class="input input-bordered w-full pl-9"
            [formControl]="searchCtrl"
          />
        </div>
        <select class="select select-bordered w-full sm:w-52" [formControl]="categoriaCtrl">
          <option value="">Todas las categorías</option>
          @for (cat of categorias(); track cat.id) {
            <option [value]="cat.id">{{ cat.name }}</option>
          }
        </select>
      </div>

      <!-- Table -->
      <app-card>
        @if (loading()) {
          <div class="flex justify-center py-16">
            <app-spinner size="lg" />
          </div>
        } @else if (productos().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 gap-4">
            <iconify-icon icon="lucide:package-x" class="text-6xl text-base-content/20"></iconify-icon>
            <div class="text-center">
              <p class="font-medium text-base-content/60">No se encontraron productos</p>
              <p class="text-sm text-base-content/40 mt-1">Agrega productos para gestionar tu inventario</p>
            </div>
            <button appBtn size="sm" (click)="productoModal.open()">
              <iconify-icon icon="lucide:plus" class="text-base"></iconify-icon>
              Agregar producto
            </button>
          </div>
        } @else {
          <app-table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th class="hidden md:table-cell">Categoría</th>
                <th class="hidden sm:table-cell">ITBIS</th>
                <th>Estado</th>
                <th class="w-8"></th>
              </tr>
            </thead>
            <tbody>
              @for (p of productos(); track p.id) {
                <tr class="hover cursor-pointer" [routerLink]="[p.id]">
                  <td class="font-medium text-base-content">{{ p.name }}</td>
                  <td class="hidden md:table-cell text-sm text-base-content/60">{{ p.categoryName }}</td>
                  <td class="hidden sm:table-cell text-sm text-base-content/60">{{ p.itbisRate | rdItbis }}</td>
                  <td>
                    <app-badge [variant]="p.isActive ? 'success' : 'ghost'">
                      {{ p.isActive ? 'Activo' : 'Inactivo' }}
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
              <p class="text-sm text-base-content/50">
                {{ totalCount() }} producto{{ totalCount() !== 1 ? 's' : '' }}
              </p>
              <div class="join">
                <button
                  class="join-item btn btn-sm"
                  [disabled]="page() === 1"
                  (click)="changePage(page() - 1)"
                >
                  <iconify-icon icon="lucide:chevron-left"></iconify-icon>
                </button>
                <span class="join-item btn btn-sm btn-active pointer-events-none">
                  {{ page() }} / {{ totalPages() }}
                </span>
                <button
                  class="join-item btn btn-sm"
                  [disabled]="page() === totalPages()"
                  (click)="changePage(page() + 1)"
                >
                  <iconify-icon icon="lucide:chevron-right"></iconify-icon>
                </button>
              </div>
            </div>
          }
        }
      </app-card>
    </div>

    <!-- Nuevo Producto Modal -->
    <app-modal #productoModal title="Nuevo producto" (closed)="resetProductoForm()">
      <form [formGroup]="productoForm" (ngSubmit)="onCreateProducto(productoModal)" class="space-y-4">
        <div class="form-control">
          <label class="label" for="prod-name">
            <span class="label-text">Nombre *</span>
          </label>
          <input
            id="prod-name"
            type="text"
            class="input input-bordered"
            formControlName="name"
            placeholder="Arroz, Aceite de cocina..."
          />
        </div>
        <div class="form-control">
          <label class="label" for="prod-cat">
            <span class="label-text">Categoría *</span>
          </label>
          <select id="prod-cat" class="select select-bordered" formControlName="categoryId">
            <option value="">Selecciona una categoría</option>
            @for (cat of categorias(); track cat.id) {
              <option [value]="cat.id">{{ cat.name }}</option>
            }
          </select>
        </div>
        <div class="form-control">
          <label class="label" for="prod-itbis">
            <span class="label-text">ITBIS</span>
          </label>
          <select id="prod-itbis" class="select select-bordered" formControlName="itbisRate">
            @for (opt of itbisOpts; track opt.value) {
              <option [value]="opt.value">{{ opt.label }}</option>
            }
          </select>
        </div>
        <div modalActions>
          <button type="button" class="btn btn-ghost" (click)="productoModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="productoForm.invalid">
            Crear producto
          </button>
        </div>
      </form>
    </app-modal>

    <!-- Nueva Categoría Modal -->
    <app-modal #categoriaModal title="Nueva categoría" (closed)="resetCategoriaForm()">
      <form [formGroup]="categoriaForm" (ngSubmit)="onCreateCategoria(categoriaModal)" class="space-y-4">
        <div class="form-control">
          <label class="label" for="cat-name">
            <span class="label-text">Nombre *</span>
          </label>
          <input
            id="cat-name"
            type="text"
            class="input input-bordered"
            formControlName="name"
            placeholder="Granos, Bebidas, Lácteos..."
          />
        </div>
        <div modalActions>
          <button type="button" class="btn btn-ghost" (click)="categoriaModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="categoriaForm.invalid">
            Crear categoría
          </button>
        </div>
      </form>
    </app-modal>
  `,
})
export class ListaInventarioPage {
  private svc = inject(InventarioService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  productos = signal<ProductoResumen[]>([]);
  categorias = signal<Categoria[]>([]);
  totalCount = signal(0);
  totalPages = signal(0);
  page = signal(1);

  searchCtrl = this.fb.nonNullable.control('');
  categoriaCtrl = this.fb.nonNullable.control('');

  productoForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    categoryId: ['', Validators.required],
    itbisRate: [0],
  });

  categoriaForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
  });

  readonly itbisOpts = ITBIS_OPTIONS;

  constructor() {
    this.loadCategorias();
    this.loadProductos();

    this.searchCtrl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntilDestroyed(),
    ).subscribe(() => { this.page.set(1); this.loadProductos(); });

    this.categoriaCtrl.valueChanges.pipe(
      takeUntilDestroyed(),
    ).subscribe(() => { this.page.set(1); this.loadProductos(); });
  }

  loadProductos(): void {
    this.loading.set(true);
    const search = this.searchCtrl.value || undefined;
    const catId = this.categoriaCtrl.value || undefined;
    this.svc.getProductos(this.page(), 20, search, catId).subscribe({
      next: (res) => {
        this.productos.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => { this.toast.error('Error cargando productos'); this.loading.set(false); },
    });
  }

  loadCategorias(): void {
    this.svc.getCategorias().subscribe({
      next: (cats) => this.categorias.set(cats.filter(c => c.isActive)),
      error: () => {},
    });
  }

  changePage(p: number): void { this.page.set(p); this.loadProductos(); }

  onCreateProducto(modal: ModalComponent): void {
    if (this.productoForm.invalid) return;
    this.saving.set(true);
    const { name, categoryId, itbisRate } = this.productoForm.getRawValue();
    this.svc.createProducto(name, categoryId, itbisRate).subscribe({
      next: () => {
        this.toast.success('Producto creado');
        modal.close();
        this.loadProductos();
        this.saving.set(false);
      },
      error: () => { this.toast.error('Error al crear producto'); this.saving.set(false); },
    });
  }

  onCreateCategoria(modal: ModalComponent): void {
    if (this.categoriaForm.invalid) return;
    this.saving.set(true);
    const { name } = this.categoriaForm.getRawValue();
    this.svc.createCategoria(name).subscribe({
      next: () => {
        this.toast.success('Categoría creada');
        modal.close();
        this.loadCategorias();
        this.saving.set(false);
      },
      error: () => { this.toast.error('Error al crear categoría'); this.saving.set(false); },
    });
  }

  resetProductoForm(): void { this.productoForm.reset({ name: '', categoryId: '', itbisRate: 0 }); }
  resetCategoriaForm(): void { this.categoriaForm.reset({ name: '' }); }
}
