import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { InventoryService, CategoryDto } from '../../../core/services/inventory.service';
import { DEFAULT_ITBIS_RATE, LOW_STOCK_THRESHOLD } from '../../../core/constants';
import type { ProductDto } from '../../../core/models/inventory.models';

function salePriceValidator(group: AbstractControl): ValidationErrors | null {
  const cost = group.get('costPrice')?.value ?? 0;
  const sale = group.get('salePrice')?.value ?? 0;
  return sale > 0 && sale < cost ? { saleBelowCost: true } : null;
}

const ERROR_MESSAGES: Record<string, string> = {
  'product.sale_price_below_cost': 'El precio de venta no puede ser menor al costo.',
  'product.category_required': 'Selecciona una categoría.',
  'product.name_required': 'El nombre del producto es obligatorio.',
  'product.invalid_price': 'El precio ingresado no es válido.',
  'category.not_found': 'La categoría seleccionada no existe.',
};

function mapInventoryError(code?: string): string {
  if (!code) return '';
  return ERROR_MESSAGES[code] ?? code;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './inventory.html',
})
export class Inventory implements OnInit {
  private inventoryService = inject(InventoryService);
  private fb = inject(FormBuilder);

  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal<string | null>(null);

  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  totalPages = signal(0);
  searchQuery = signal('');

  modalMode = signal<'create' | null>(null);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    costPrice: [0, [Validators.required, Validators.min(0)]],
    salePrice: [0, [Validators.required, Validators.min(0.01)]],
    itbisRate: [DEFAULT_ITBIS_RATE, [Validators.required, Validators.min(0), Validators.max(1)]],
    categoryId: ['', Validators.required],
  }, { validators: salePriceValidator });

  totalProducts = computed(() => this.totalCount());
  lowStockProducts = computed(() => {
    let count = 0;
    for (const p of this.products()) {
      for (const pres of p.presentations ?? []) {
        if (pres.stockQuantity <= LOW_STOCK_THRESHOLD) { count++; break; }
      }
    }
    return count;
  });
  activeProducts = computed(() => this.products().filter(p => p.isActive).length);

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadingCategories = signal(false);

  loadCategories(): void {
    this.loadingCategories.set(true);
    this.inventoryService.getCategories().subscribe({
      next: (cats) => { this.categories.set(cats); this.loadingCategories.set(false); },
      error: () => this.loadingCategories.set(false),
    });
  }

  loadProducts(): void {
    this.loading.set(true);
    this.errorMsg.set(null);
    this.inventoryService.getProducts(this.page(), this.pageSize(), this.searchQuery() || undefined).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMsg.set(err?.error?.message || 'Error cargando productos.');
        this.loading.set(false);
      }
    });
  }

  onSearch(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchQuery.set(val);
    this.page.set(1);
    this.loadProducts();
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update(p => p + 1);
      this.loadProducts();
    }
  }

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.loadProducts();
    }
  }

  openCreate(): void {
    this.errorMsg.set(null);
    this.modalMode.set('create');
    this.loadingCategories.set(true);
    this.inventoryService.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        this.loadingCategories.set(false);
        this.createForm.reset({ itbisRate: DEFAULT_ITBIS_RATE, categoryId: cats[0]?.id ?? '' });
      },
      error: () => this.loadingCategories.set(false),
    });
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.errorMsg.set(null);
  }

  submitCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.getRawValue();
    this.inventoryService.createProduct({
      name: v.name,
      categoryId: v.categoryId,
      itbisRate: v.itbisRate,
    }).subscribe({
      next: (res) => {
        this.inventoryService.addPresentation(res.productId, {
          displayName: 'Predeterminada',
          presentationType: 2,
          sellMode: 1,
          measureUnit: 1,
          salePrice: v.salePrice,
          costPrice: v.costPrice,
        }).subscribe({
          next: () => {
            this.saving.set(false);
            this.closeModal();
            this.loadProducts();
          },
          error: () => {
            this.saving.set(false);
            this.closeModal();
            this.loadProducts();
          },
        });
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(mapInventoryError(err?.error?.error ?? err?.error?.message) || 'Error creando producto.');
      }
    });
  }

  deactivate(product: ProductDto): void {
    if (!confirm(`¿Desactivar "${product.name}"? Ya no aparecerá en ventas.`)) return;
    this.inventoryService.deactivateProduct(product.productId).subscribe({
      next: () => this.loadProducts(),
      error: (err) => alert(err?.error?.message || 'Error desactivando producto.')
    });
  }

  getStockSummary(product: ProductDto): string {
    const presentations = product.presentations ?? [];
    const bulk = presentations.filter(p => p.presentationType === 1);
    const packaged = presentations.filter(p => p.presentationType === 2);
    const parts: string[] = [];
    if (packaged.length > 0) {
      const total = packaged.reduce((s, p) => s + p.packagedStockQuantity, 0);
      parts.push(`${total} empacadas`);
    }
    if (bulk.length > 0) {
      const total = bulk.reduce((s, p) => s + p.openContainersCount, 0);
      parts.push(`${total} contenedores`);
    }
    return parts.length > 0 ? parts.join(', ') : 'Sin stock';
  }

  getPriceRange(product: ProductDto): string {
    const presentations = product.presentations ?? [];
    if (presentations.length === 0) return '—';
    if (presentations.length === 1) {
      return presentations[0].salePrice.toFixed(2);
    }
    const prices = presentations.map(p => p.salePrice);
    const min = Math.min(...prices);
    const max = Math.max(...prices);
    return `${min.toFixed(2)} — ${max.toFixed(2)}`;
  }

  getStockClass(product: ProductDto): string {
    const minStock = Math.min(...(product.presentations ?? []).map(p => p.stockQuantity));
    if (minStock <= 0) return 'text-red-500';
    if (minStock <= LOW_STOCK_THRESHOLD) return 'text-amber-400';
    return 'text-green-400';
  }
}
