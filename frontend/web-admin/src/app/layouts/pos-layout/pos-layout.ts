import { Component, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import {
  SaleService, ShiftDto, CreateSaleResult
} from '../../core/services/sale.service';
import { InventoryService, ProductDto } from '../../core/services/inventory.service';
import { SettingsService, TenantProfileDto } from '../../core/services/settings.service';

export interface CartItem {
  productId: string;
  name: string;
  salePrice: number;
  itbisRate: number;
  quantity: number;
  lineSubtotal: number;
  lineItbis: number;
  lineTotal: number;
}

/** Payment method IDs as defined in the backend PaymentMethod enum. */
const PM = { Cash: 1, Card: 2, Transfer: 3 } as const;

@Component({
  selector: 'app-pos-layout',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './pos-layout.html',
  styleUrl: './pos-layout.scss',
})
export class PosLayout implements OnInit, OnDestroy {
  private auth     = inject(AuthService);
  private sales    = inject(SaleService);
  private inv      = inject(InventoryService);
  private settings = inject(SettingsService);
  private router   = inject(Router);
  private fb       = inject(FormBuilder);

  // ── Exposed for template ──────────────────────────
  authService = this.auth;

  // ── Data signals ─────────────────────────────────
  catalog       = signal<ProductDto[]>([]);
  cartItems     = signal<CartItem[]>([]);
  activeShift   = signal<ShiftDto | null>(null);
  lastSale      = signal<CreateSaleResult | null>(null);
  tenantProfile = signal<TenantProfileDto | null>(null);

  // ── UI state ─────────────────────────────────────
  loading            = signal(false);
  saving             = signal(false);
  catalogSearch      = signal('');
  selectedCategory   = signal<string | null>(null);
  showOpenShiftModal  = signal(false);
  showCloseShiftModal = signal(false);
  showPaymentModal    = signal(false);
  showReceiptModal    = signal(false);
  errorMsg           = signal<string | null>(null);

  // ── Payment state ─────────────────────────────────
  paymentMethod  = signal<'cash' | 'card' | 'transfer'>('cash');
  cashTendered   = signal(0);
  buyerRnc       = signal('');

  // ── Clock ─────────────────────────────────────────
  clock = signal(new Date());
  private clockTimer?: ReturnType<typeof setInterval>;

  // ── Forms ─────────────────────────────────────────
  openShiftForm = this.fb.group({
    cashierName:       ['', [Validators.required, Validators.minLength(3)]],
    openingCashAmount: [0,  [Validators.required, Validators.min(0)]]
  });

  closeShiftForm = this.fb.group({
    actualCashAmount: [0, [Validators.required, Validators.min(0)]],
    notes: ['']
  });

  // ── Computed ──────────────────────────────────────
  filteredCatalog = computed(() => {
    let items = this.catalog();
    const q   = this.catalogSearch().toLowerCase().trim();
    const cat = this.selectedCategory();
    if (q)   items = items.filter(p => p.name.toLowerCase().includes(q));
    if (cat) items = items.filter(p => p.categoryId === cat);
    return items;
  });

  categories = computed(() => {
    const seen = new Map<string, string>();
    for (const p of this.catalog()) {
      if (!seen.has(p.categoryId)) seen.set(p.categoryId, p.categoryName);
    }
    return Array.from(seen.entries()).map(([id, name]) => ({ id, name }));
  });

  cartSubtotal = computed(() => this.cartItems().reduce((s, i) => s + i.lineSubtotal, 0));
  cartItbis    = computed(() => this.cartItems().reduce((s, i) => s + i.lineItbis, 0));
  cartTotal    = computed(() => this.cartItems().reduce((s, i) => s + i.lineTotal, 0));
  cartCount    = computed(() => this.cartItems().reduce((s, i) => s + i.quantity, 0));

  changeDue = computed(() => {
    if (this.paymentMethod() === 'cash') {
      return Math.max(0, this.cashTendered() - this.cartTotal());
    }
    return 0;
  });

  itbisBreakdown = computed(() => {
    const m = new Map<number, number>();
    for (const item of this.cartItems()) {
      const r = item.itbisRate;
      if (r > 0) m.set(r, (m.get(r) ?? 0) + item.lineItbis);
    }
    return Array.from(m.entries()).map(([rate, amount]) => ({
      label:  `ITBIS ${(rate * 100).toFixed(0)}%`,
      amount
    }));
  });

  // ── Lifecycle ─────────────────────────────────────
  ngOnInit(): void {
    this.loadCatalog();
    this.loadShift();
    this.loadTenantProfile();
    this.clockTimer = setInterval(() => this.clock.set(new Date()), 1000);
    // Pre-fill cashier name
    const u = this.auth.currentUser();
    if (u) {
      const name = `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.email;
      this.openShiftForm.patchValue({ cashierName: name });
    }
  }

  ngOnDestroy(): void {
    if (this.clockTimer) clearInterval(this.clockTimer);
  }

  // ── Data loading ──────────────────────────────────
  loadCatalog(): void {
    this.loading.set(true);
    this.inv.getCatalog().subscribe({
      next: products => {
        this.catalog.set(products.filter(p => p.isActive));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadShift(): void {
    this.sales.getCurrentShift().subscribe({
      next: shift => this.activeShift.set(shift),
      error: () => this.activeShift.set(null)
    });
  }

  loadTenantProfile(): void {
    this.settings.getProfile().subscribe({
      next: profile => this.tenantProfile.set(profile),
      error: () => {} // non-critical — receipt falls back to generic header
    });
  }

  // ── Cart operations ───────────────────────────────
  addToCart(product: ProductDto): void {
    if (!this.activeShift()) {
      this.showOpenShiftModal.set(true);
      return;
    }
    const items = this.cartItems();
    const idx   = items.findIndex(i => i.productId === product.productId);
    if (idx >= 0) {
      this.setQuantity(idx, items[idx].quantity + 1);
    } else {
      const sub   = product.salePrice;
      const itbis = sub * product.itbisRate;
      this.cartItems.set([...items, {
        productId:   product.productId,
        name:        product.name,
        salePrice:   product.salePrice,
        itbisRate:   product.itbisRate,
        quantity:    1,
        lineSubtotal: sub,
        lineItbis:   itbis,
        lineTotal:   sub + itbis
      }]);
    }
    this.syncCashTendered();
  }

  setQuantity(index: number, qty: number): void {
    if (qty <= 0) { this.removeItem(index); return; }
    const items   = [...this.cartItems()];
    const item    = items[index];
    const sub     = item.salePrice * qty;
    const itbis   = sub * item.itbisRate;
    items[index]  = { ...item, quantity: qty, lineSubtotal: sub, lineItbis: itbis, lineTotal: sub + itbis };
    this.cartItems.set(items);
    this.syncCashTendered();
  }

  removeItem(index: number): void {
    this.cartItems.update(items => items.filter((_, i) => i !== index));
    this.syncCashTendered();
  }

  clearCart(): void {
    this.cartItems.set([]);
    this.cashTendered.set(0);
    this.buyerRnc.set('');
  }

  private syncCashTendered(): void {
    // Round up to nearest 100 for convenience
    const total  = this.cartTotal();
    const rounded = Math.ceil(total / 100) * 100;
    this.cashTendered.set(rounded);
  }

  // ── Shift ─────────────────────────────────────────
  submitOpenShift(): void {
    if (this.openShiftForm.invalid || this.saving()) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.openShiftForm.value;
    this.sales.openShift({
      cashierName:       v.cashierName!,
      openingCashAmount: v.openingCashAmount!
    }).subscribe({
      next: () => {
        this.showOpenShiftModal.set(false);
        this.loadShift();
        this.saving.set(false);
      },
      error: (e) => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al abrir el turno.');
        this.saving.set(false);
      }
    });
  }

  submitCloseShift(): void {
    const shift = this.activeShift();
    if (!shift || this.closeShiftForm.invalid || this.saving()) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.closeShiftForm.value;
    this.sales.closeShift(shift.shiftId, {
      actualCashAmount: v.actualCashAmount!,
      notes: v.notes || undefined
    }).subscribe({
      next: () => {
        this.showCloseShiftModal.set(false);
        this.activeShift.set(null);
        this.saving.set(false);
      },
      error: (e) => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al cerrar el turno.');
        this.saving.set(false);
      }
    });
  }

  // ── Checkout ──────────────────────────────────────
  openPaymentModal(): void {
    if (!this.cartItems().length || !this.activeShift()) return;
    this.syncCashTendered();
    this.errorMsg.set(null);
    this.showPaymentModal.set(true);
  }

  submitSale(): void {
    if (this.saving() || !this.cartItems().length) return;
    const total   = this.cartTotal();
    const paid    = this.paymentMethod() === 'cash' ? this.cashTendered() : total;
    if (paid < total) {
      this.errorMsg.set('El monto recibido es menor al total.');
      return;
    }
    this.saving.set(true);
    this.errorMsg.set(null);
    const methodId = this.paymentMethod() === 'cash' ? PM.Cash
                   : this.paymentMethod() === 'card' ? PM.Card
                   : PM.Transfer;
    this.sales.createSale({
      items:    this.cartItems().map(i => ({ productId: i.productId, quantity: i.quantity })),
      payments: [{ paymentMethodId: methodId, amount: paid }],
      buyerRnc: this.buyerRnc() || null,
      notes:    null
    }).subscribe({
      next: result => {
        this.lastSale.set(result);
        this.showPaymentModal.set(false);
        this.showReceiptModal.set(true);
        this.clearCart();
        this.saving.set(false);
      },
      error: (e) => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al registrar la venta.');
        this.saving.set(false);
      }
    });
  }

  printReceipt(): void {
    window.print();
  }

  // ── Helpers ───────────────────────────────────────
  pesos(v: number): string {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency', currency: 'DOP', minimumFractionDigits: 2
    }).format(v);
  }

  fmtDate(d: Date | string = new Date()): string {
    return new Intl.DateTimeFormat('es-DO', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    }).format(new Date(d));
  }

  cashierName(): string {
    const u = this.auth.currentUser();
    return u ? `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.email : 'Cajero';
  }

  logout(): void { this.auth.logout(); }
  toPortal(): void { this.router.navigate(['/portal/dashboard']); }

  setSearch(v: string): void { this.catalogSearch.set(v); }
  selectCategory(id: string | null): void { this.selectedCategory.set(id); }
  setPaymentMethod(m: 'cash' | 'card' | 'transfer'): void { this.paymentMethod.set(m); }
  setCashTendered(v: number): void { this.cashTendered.set(v); }
  setBuyerRnc(v: string): void { this.buyerRnc.set(v); }
}
