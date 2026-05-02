import { Component, signal, computed, inject, OnInit, OnDestroy, HostListener, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import {
  SaleService, ShiftDto, CreateSaleResult
} from '../../core/services/sale.service';
import { InventoryService, ProductDto } from '../../core/services/inventory.service';
import { CustomerService, CustomerSummary } from '../../core/services/customer.service';
import { SettingsService, TenantProfileDto } from '../../core/services/settings.service';
import { RdCurrencyPipe, RncPipe } from '../../core/pipes';

export interface CartItem {
  productId: string;
  name: string;
  salePrice: number;
  itbisRate: number;
  unitTypeId: number;
  quantity: number;
  lineSubtotal: number;
  lineItbis: number;
  lineTotal: number;
}

/** Payment method IDs as defined in the backend PaymentMethod enum. */
const PM = { Cash: 1, Card: 2, Transfer: 3, Credit: 4, Delivery: 5 } as const;

@Component({
  selector: 'app-pos-layout',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RdCurrencyPipe, RncPipe],
  templateUrl: './pos-layout.html',
  styleUrl: './pos-layout.scss',
})
export class PosLayout implements OnInit, OnDestroy {
  private auth      = inject(AuthService);
  private sales     = inject(SaleService);
  private inv       = inject(InventoryService);
  private customers = inject(CustomerService);
  private settings  = inject(SettingsService);
  private router    = inject(Router);
  private fb        = inject(FormBuilder);

  // ── Exposed for template ──────────────────────────
  authService = this.auth;

  @ViewChild('quickSaleAmountInput') quickSaleAmountInput?: ElementRef<HTMLInputElement>;
  @ViewChild('cashTenderedInput') cashTenderedInput?: ElementRef<HTMLInputElement>;

  // ── Data signals ─────────────────────────────────
  catalog       = signal<ProductDto[]>([]);
  cartItems     = signal<CartItem[]>([]);
  activeShift   = signal<ShiftDto | null>(null);
  lastSale      = signal<CreateSaleResult | null>(null);
  lastSalePaymentMethod = signal<string>('');
  lastSaleCustomerName  = signal<string>('');
  lastSaleCustomerPhone = signal<string>('');
  tenantProfile = signal<TenantProfileDto | null>(null);
  allCustomers  = signal<CustomerSummary[]>([]);

  // ── UI state ─────────────────────────────────────
  loading             = signal(false);
  saving              = signal(false);
  catalogSearch       = signal('');
  selectedCategory    = signal<string | null>(null);
  showOpenShiftModal   = signal(false);
  showCloseShiftModal  = signal(false);
  showPaymentModal     = signal(false);
  showReceiptModal     = signal(false);
  showAddCustomerModal = signal(false);
  showQuickSaleModal   = signal(false);
  errorMsg            = signal<string | null>(null);

  // ── Payment state ─────────────────────────────────
  paymentMethod  = signal<'cash' | 'card' | 'transfer' | 'credit' | 'delivery'>('cash');
  cashTendered   = signal(0);
  buyerRnc       = signal('');
  selectedCustomer = signal<CustomerSummary | null>(null);
  customerSearchQuery = signal('');

  // ── Geocoding (Nominatim) ─────────────────────────
  geocodeLat     = signal<number | null>(null);
  geocodeLon     = signal<number | null>(null);
  geocodeLoading = signal(false);
  geocodeError   = signal<string | null>(null);

  // ── Quick Sale ────────────────────────────────────
  quickSaleSearch = signal('');
  quickSaleAmount = signal<number | null>(null);
  selectedQuickProduct = signal<ProductDto | null>(null);

  // ── Catalog pagination ────────────────────────────
  readonly PAGE_SIZE = 24;
  catalogPage = signal(0);

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.key === 'F2') {
      event.preventDefault();
      if (this.showQuickSaleModal()) {
         this.closeModals();
      } else {
         this.openQuickSaleModal();
      }
    } else if (event.key === 'F4') {
      event.preventDefault();
      if (!this.showPaymentModal()) this.openPaymentModal();
    } else if (event.key === 'F8') {
      event.preventDefault();
      if (this.showReceiptModal()) this.printReceipt();
    } else if (event.key === 'F9') {
      event.preventDefault();
      if (this.showPaymentModal()) this.submitSale();
    } else if (event.key === 'Escape') {
      this.closeModals();
    }
  }

  closeModals(): void {
    this.showQuickSaleModal.set(false);
    this.showPaymentModal.set(false);
    this.showReceiptModal.set(false);
    this.showAddCustomerModal.set(false);
    this.showOpenShiftModal.set(false);
    this.showCloseShiftModal.set(false);
  }

  openQuickSaleModal(): void {
    if (!this.activeShift()) {
      this.showOpenShiftModal.set(true);
      return;
    }
    this.showQuickSaleModal.set(true);
    this.quickSaleAmount.set(null);
    this.quickSaleSearch.set('');
    this.selectedQuickProduct.set(null);
    setTimeout(() => this.quickSaleAmountInput?.nativeElement?.focus(), 100);
  }

  topQuickProducts = computed(() => {
    const q = this.quickSaleSearch().toLowerCase().trim();
    const sorted = [...this.catalog()].sort((a, b) => b.stockQuantity - a.stockQuantity);
    if (!q) return sorted.slice(0, 8);
    return sorted.filter(p => p.name.toLowerCase().includes(q)).slice(0, 8);
  });

  submitQuickSale(): void {
    const p = this.selectedQuickProduct();
    const amt = this.quickSaleAmount();
    if (!p || !amt || amt <= 0) return;

    // Calculate quantity based on the amount the user wants to pay
    // Price = SalePrice + (SalePrice * ItbisRate) = SalePrice * (1 + ItbisRate)
    const unitPriceWithTax = p.salePrice * (1 + p.itbisRate);
    const quantity = amt / unitPriceWithTax;

    this.addToCart(p, quantity);
    this.closeModals();
  }

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

  customerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    phone:    [''],
    documentId: ['', [Validators.required]],
    creditLimit: [5000, [Validators.required, Validators.min(0)]],
    province: [''],
    sector: [''],
    street: [''],
    houseNumber: [''],
    reference: ['']
  });

  deliveryAddressForm = this.fb.group({
    province: ['', Validators.required],
    sector: ['', Validators.required],
    street: ['', Validators.required],
    houseNumber: [''],
    reference: ['', Validators.required]
  });

  expectedCash = computed(() => {
    const s = this.activeShift();
    if (!s) return 0;
    return s.openingCashAmount + s.totalSalesAmount;
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

  filteredCustomers = computed(() => {
    const q = this.customerSearchQuery().toLowerCase().trim();
    if (!q) return this.allCustomers().slice(0, 10);
    return this.allCustomers().filter(c => 
      c.fullName.toLowerCase().includes(q) || 
      c.phone?.includes(q)
    ).slice(0, 10);
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

  whatsAppDeliveryUrl = computed(() => {
    const sale = this.lastSale();
    const phone = this.lastSaleCustomerPhone().replace(/\D/g, '');
    const name  = this.lastSaleCustomerName();
    if (!sale?.confirmationCode || !phone) return null;
    const total = new Intl.NumberFormat('es-DO', { style: 'currency', currency: 'DOP' }).format(sale.total);
    const text  = encodeURIComponent(
      `Hola ${name}! Tu pedido de ${total} está en camino. Tu código de confirmación de entrega es: *${sale.confirmationCode}*. Dáselo al repartidor cuando recibas el pedido.`
    );
    return `https://wa.me/${phone}?text=${text}`;
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
    this.loadCustomers();
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

  loadCustomers(): void {
    this.customers.getCustomers().subscribe({
      next: data => this.allCustomers.set(data),
      error: () => {}
    });
  }

  // ── Cart operations ───────────────────────────────
  updateStock(productId: string, delta: number): void {
    this.catalog.update(cats => cats.map(p => 
      p.productId === productId ? { ...p, stockQuantity: p.stockQuantity + delta } : p
    ));
  }

  addToCart(product: ProductDto, customQuantity?: number): void {
    if (!this.activeShift()) {
      this.showOpenShiftModal.set(true);
      return;
    }
    
    const qtyToAdd = customQuantity ?? 1;
    const items = this.cartItems();
    const idx   = items.findIndex(i => i.productId === product.productId);
    
    if (idx >= 0) {
      this.setQuantity(idx, items[idx].quantity + qtyToAdd);
    } else {
      const sub   = product.salePrice * qtyToAdd;
      const itbis = sub * product.itbisRate;
      this.cartItems.set([...items, {
        productId:   product.productId,
        name:        product.name,
        salePrice:   product.salePrice,
        itbisRate:   product.itbisRate,
        unitTypeId:  product.unitTypeId,
        quantity:    qtyToAdd,
        lineSubtotal: sub,
        lineItbis:   itbis,
        lineTotal:   sub + itbis
      }]);
      this.updateStock(product.productId, -qtyToAdd);
    }
    this.syncCashTendered();
  }

  setQuantity(index: number, qty: number): void {
    if (qty <= 0) { this.removeItem(index); return; }
    const items   = [...this.cartItems()];
    const item    = items[index];
    
    const delta   = qty - item.quantity;
    
    const sub     = item.salePrice * qty;
    const itbis   = sub * item.itbisRate;
    items[index]  = { ...item, quantity: qty, lineSubtotal: sub, lineItbis: itbis, lineTotal: sub + itbis };
    this.cartItems.set(items);
    
    this.updateStock(item.productId, -delta);
    this.syncCashTendered();
  }

  removeItem(index: number): void {
    const item = this.cartItems()[index];
    this.updateStock(item.productId, item.quantity);
    this.cartItems.update(items => items.filter((_, i) => i !== index));
    this.syncCashTendered();
  }

  cancelSale(): void {
    for (const item of this.cartItems()) {
      this.updateStock(item.productId, item.quantity);
    }
    this.clearCart();
  }

  clearCart(): void {
    this.cartItems.set([]);
    this.cashTendered.set(0);
    this.buyerRnc.set('');
    this.selectedCustomer.set(null);
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

  openCloseShiftModal(): void {
    this.closeShiftForm.patchValue({ actualCashAmount: this.expectedCash(), notes: '' });
    this.errorMsg.set(null);
    this.showCloseShiftModal.set(true);
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

  // ── Customers ─────────────────────────────────────
  selectCustomer(c: CustomerSummary): void {
    this.selectedCustomer.set(c);
    this.customerSearchQuery.set('');
    // Auto-geocode when selected for delivery and customer has address but no coords
    if (this.paymentMethod() === 'delivery' && c.province && c.street && c.sector && !c.latitude) {
      this.geocodeCustomerAddress(c);
    }
  }

  private async geocodeCustomerAddress(c: CustomerSummary): Promise<void> {
    const query = encodeURIComponent(
      `${c.street}${c.houseNumber ? ' ' + c.houseNumber : ''}, ${c.sector}, ${c.province}, República Dominicana`
    );
    try {
      const res  = await fetch(`https://nominatim.openstreetmap.org/search?q=${query}&format=json&limit=1&countrycodes=do`);
      const data = await res.json();
      if (data?.length > 0) {
        this.selectedCustomer.update(prev => prev
          ? { ...prev, latitude: parseFloat(data[0].lat), longitude: parseFloat(data[0].lon) }
          : prev
        );
      }
    } catch { /* silent — delivery still works without GPS */ }
  }

  submitAddCustomer(): void {
    if (this.customerForm.invalid || this.saving()) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.customerForm.value;
    
    let address = null;
    if (v.province && v.sector && v.street && v.reference) {
      address = {
        province: v.province,
        sector: v.sector,
        street: v.street,
        reference: v.reference,
        houseNumber: v.houseNumber || undefined
      };
    }

    this.customers.createCustomer({
      fullName: v.fullName!,
      documentId: v.documentId!,
      phone: v.phone || null,
      creditLimit: v.creditLimit!,
      address: address
    }).subscribe({
      next: res => {
        this.loadCustomers();
        // Auto-select the newly created customer
        this.selectedCustomer.set({
          customerId: res.customerId,
          fullName: v.fullName!,
          phone: v.phone || '',
          balance: 0,
          creditLimit: v.creditLimit!,
          isActive: true,
          province: v.province,
          sector: v.sector,
          street: v.street,
          reference: v.reference,
          houseNumber: v.houseNumber || null
        });
        this.showAddCustomerModal.set(false);
        this.customerForm.reset({ creditLimit: 5000 });
        this.saving.set(false);
      },
      error: e => {
        this.errorMsg.set(e?.error?.detail ?? 'Error al crear cliente.');
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
    
    // Validations for Credit
    if (this.paymentMethod() === 'credit' && !this.selectedCustomer()) {
      this.errorMsg.set('Debes seleccionar un cliente para ventas a crédito.');
      return;
    }

    const paid    = this.paymentMethod() === 'cash' ? this.cashTendered() : total;
    if (paid < total && this.paymentMethod() !== 'credit' && this.paymentMethod() !== 'delivery') {
      this.errorMsg.set('El monto recibido es menor al total.');
      return;
    }

    this.saving.set(true);
    this.errorMsg.set(null);
    
    const methodId = this.paymentMethod() === 'cash' ? PM.Cash
                   : this.paymentMethod() === 'card' ? PM.Card
                   : this.paymentMethod() === 'transfer' ? PM.Transfer
                   : this.paymentMethod() === 'credit' ? PM.Credit
                   : PM.Delivery;

    let deliveryAddress = null;
    if (this.paymentMethod() === 'delivery') {
      const c = this.selectedCustomer();
      if (c && c.province && c.sector && c.street && c.reference) {
        deliveryAddress = {
          province: c.province,
          sector: c.sector,
          street: c.street,
          reference: c.reference,
          houseNumber: c.houseNumber || undefined,
          latitude: c.latitude || undefined,
          longitude: c.longitude || undefined
        };
      } else if (this.deliveryAddressForm.valid) {
        const v = this.deliveryAddressForm.value;
        deliveryAddress = {
          province: v.province!,
          sector: v.sector!,
          street: v.street!,
          reference: v.reference!,
          houseNumber: v.houseNumber || undefined,
          latitude: this.geocodeLat() ?? undefined,
          longitude: this.geocodeLon() ?? undefined
        };
      } else {
        this.errorMsg.set('Para delivery, selecciona un cliente con dirección o llena los datos de envío.');
        this.saving.set(false);
        return;
      }
    }

    this.sales.createSale({
      items:    this.cartItems().map(i => ({ productId: i.productId, quantity: i.quantity })),
      payments: [{ 
        paymentMethodId: methodId, 
        amount: total, // For credit/delivery, the amount registered is the total
        customerId: this.selectedCustomer()?.customerId 
      }],
      buyerRnc: this.buyerRnc() || null,
      deliveryAddress: deliveryAddress,
      notes:    this.paymentMethod() === 'credit' ? `Fiado a ${this.selectedCustomer()?.fullName}` : null
    }).subscribe({
      next: result => {
        this.lastSale.set(result);
        this.lastSalePaymentMethod.set(this.paymentMethod());
        this.lastSaleCustomerName.set(this.selectedCustomer()?.fullName ?? '');
        this.lastSaleCustomerPhone.set(this.selectedCustomer()?.phone ?? '');
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

  async geocodeAddress(): Promise<void> {
    const v = this.deliveryAddressForm.value;
    if (!v.street || !v.sector || !v.province) {
      this.geocodeError.set('Completa al menos calle, sector y provincia.');
      return;
    }
    this.geocodeLoading.set(true);
    this.geocodeError.set(null);
    this.geocodeLat.set(null);
    this.geocodeLon.set(null);

    const query = encodeURIComponent(
      `${v.street}${v.houseNumber ? ' ' + v.houseNumber : ''}, ${v.sector}, ${v.province}, República Dominicana`
    );

    try {
      const res = await fetch(
        `https://nominatim.openstreetmap.org/search?q=${query}&format=json&limit=1&countrycodes=do`,
        { headers: { 'Accept-Language': 'es' } }
      );
      const data = await res.json();
      if (data?.length > 0) {
        this.geocodeLat.set(parseFloat(data[0].lat));
        this.geocodeLon.set(parseFloat(data[0].lon));
      } else {
        this.geocodeError.set('No se encontraron coordenadas. La entrega será sin verificación GPS.');
      }
    } catch {
      this.geocodeError.set('Error al obtener coordenadas.');
    } finally {
      this.geocodeLoading.set(false);
    }
  }

  // ── Helpers ───────────────────────────────────────
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

  totalPages    = computed(() => Math.max(1, Math.ceil(this.filteredCatalog().length / this.PAGE_SIZE)));
  pagedCatalog  = computed(() => {
    const p = this.catalogPage();
    return this.filteredCatalog().slice(p * this.PAGE_SIZE, (p + 1) * this.PAGE_SIZE);
  });

  setSearch(v: string): void { this.catalogSearch.set(v); this.catalogPage.set(0); }
  selectCategory(id: string | null): void { this.selectedCategory.set(id); this.catalogPage.set(0); }
  prevPage(): void { this.catalogPage.update(p => Math.max(0, p - 1)); }
  nextPage(): void { this.catalogPage.update(p => Math.min(this.totalPages() - 1, p + 1)); }

  productInitials(name: string): string {
    return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase();
  }

  productColor(categoryName: string): string {
    const colors = ['bg-blue-700','bg-violet-700','bg-emerald-700','bg-amber-700','bg-rose-700','bg-cyan-700','bg-indigo-700','bg-teal-700'];
    let hash = 0;
    for (let i = 0; i < categoryName.length; i++) hash = categoryName.charCodeAt(i) + ((hash << 5) - hash);
    return colors[Math.abs(hash) % colors.length];
  }
  setPaymentMethod(m: 'cash' | 'card' | 'transfer' | 'credit' | 'delivery'): void { this.paymentMethod.set(m); }
  setCashTendered(v: number): void { this.cashTendered.set(v); }
  setBuyerRnc(v: string): void { this.buyerRnc.set(v); }
}
