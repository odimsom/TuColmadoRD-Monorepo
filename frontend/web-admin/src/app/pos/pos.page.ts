import {
  Component, ChangeDetectionStrategy, inject, signal, computed, viewChild, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PosService } from './services/pos.service';
import { AuthService } from '../core/auth.service';
import { ToastService } from '../shared/ui/toast/toast.service';
import { SpinnerComponent } from '../shared/ui/spinner/spinner.component';
import { ModalComponent } from '../shared/ui/modal/modal.component';
import { BtnComponent } from '../shared/ui/btn/btn.component';
import { BadgeComponent } from '../shared/ui/badge/badge.component';
import { RdCurrencyPipe } from '../shared/ui/pipes/rd-currency.pipe';
import {
  CatalogItem, CatalogPresentation, CartLine, Shift, ShiftSummary,
  CloseShiftResult, CreateSaleResult, PaymentMode, PAYMENT_METHODS,
} from './models/pos.model';

@Component({
  selector: 'app-pos-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, SpinnerComponent, ModalComponent, BtnComponent, BadgeComponent, RdCurrencyPipe],
  template: `
    @if (loading()) {
      <div class="min-h-[60vh] flex items-center justify-center"><app-spinner size="lg" /></div>

    } @else if (!shift()) {
      <!-- Sin turno abierto -->
      <div class="min-h-[calc(100vh-var(--topbar-h)-48px)] flex items-center justify-center p-6">
        <div class="tc-card max-w-md w-full text-center p-12 shadow-2xl">
          <div class="w-16 h-16 bg-primary/10 flex items-center justify-center rounded-full mx-auto mb-6">
            <iconify-icon icon="lucide:scan-barcode" class="text-primary text-3xl"></iconify-icon>
          </div>
          <h1 class="text-3xl font-black text-base-content tracking-tighter uppercase mb-3">Punto de venta</h1>
          <p class="text-base-content/40 text-sm mb-8">Abre un turno de caja para empezar a vender</p>
          <button appBtn (click)="openShiftModal.open()">
            <iconify-icon icon="lucide:play" class="text-base"></iconify-icon>
            Abrir turno
          </button>
        </div>
      </div>

    } @else {
      <!-- POS activo -->
      <div class="grid grid-cols-1 lg:grid-cols-[1fr_380px] gap-4 items-start">

        <!-- Catálogo -->
        <div class="space-y-3">
          <div class="flex flex-wrap items-center gap-2">
            <input
              type="search" class="tc-input flex-1 min-w-48" placeholder="Buscar producto..."
              [value]="search()" (input)="search.set($any($event.target).value)"
            />
            <button appBtn variant="ghost" size="sm" (click)="onOpenCloseModal()">
              <iconify-icon icon="lucide:square" class="text-base"></iconify-icon>
              Cerrar turno
            </button>
          </div>

          <div class="flex flex-wrap gap-1.5">
            <button
              class="tc-btn tc-btn-sm" [class.tc-btn-primary]="categoria() === null"
              (click)="categoria.set(null)"
            >Todas</button>
            @for (c of categorias(); track c.id) {
              <button
                class="tc-btn tc-btn-sm" [class.tc-btn-primary]="categoria() === c.id"
                (click)="categoria.set(c.id)"
              >{{ c.name }}</button>
            }
          </div>

          @if (filteredItems().length === 0) {
            <div class="flex flex-col items-center py-16 gap-3">
              <iconify-icon icon="lucide:package-open" class="text-5xl text-base-content/20"></iconify-icon>
              <p class="text-base-content/40 text-sm">No hay productos que coincidan</p>
            </div>
          } @else {
            <div class="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-3">
              @for (item of filteredItems(); track item.productId) {
                @for (p of item.presentations; track p.id) {
                  <button
                    type="button"
                    class="tc-card text-left p-4 hover:border-primary/40 hover:bg-primary/5 transition-colors"
                    (click)="addToCart(item, p)"
                  >
                    <p class="font-bold text-sm text-base-content leading-tight line-clamp-2">{{ p.displayName }}</p>
                    <p class="text-[11px] text-base-content/40 uppercase tracking-wider mt-0.5">{{ item.categoryName }}</p>
                    <p class="text-lg font-black text-primary mt-2">{{ p.salePrice | rdCurrency }}</p>
                  </button>
                }
              }
            </div>
          }
        </div>

        <!-- Carrito -->
        <div class="tc-card p-4 space-y-3 lg:sticky lg:top-4">
          <div class="flex items-center justify-between">
            <h2 class="font-black text-base-content uppercase tracking-tight">Carrito</h2>
            <app-badge variant="ghost">Turno: {{ shift()!.cashierName }}</app-badge>
          </div>

          @if (cart().length === 0) {
            <p class="text-sm text-base-content/40 text-center py-8">Toca un producto para agregarlo</p>
          } @else {
            <div class="space-y-2 max-h-[45vh] overflow-y-auto">
              @for (line of cart(); track line.presentation.id) {
                <div class="flex items-center gap-2 border-b border-base-300 pb-2">
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-bold truncate">{{ line.presentation.displayName }}</p>
                    <p class="text-xs text-base-content/40">{{ line.presentation.salePrice | rdCurrency }} c/u</p>
                  </div>
                  <div class="join">
                    <button class="tc-btn tc-btn-sm join-item" (click)="setQty(line, line.quantity - 1)" aria-label="Menos">−</button>
                    <span class="join-item px-2 text-sm font-bold self-center w-8 text-center">{{ line.quantity }}</span>
                    <button class="tc-btn tc-btn-sm join-item" (click)="setQty(line, line.quantity + 1)" aria-label="Más">+</button>
                  </div>
                  <p class="text-sm font-black w-20 text-right">{{ line.presentation.salePrice * line.quantity | rdCurrency }}</p>
                  <button class="tc-btn tc-btn-ghost tc-btn-sm" (click)="removeLine(line)" aria-label="Quitar">
                    <iconify-icon icon="lucide:x"></iconify-icon>
                  </button>
                </div>
              }
            </div>

            <div class="space-y-1 pt-1 text-sm">
              <div class="flex justify-between text-base-content/60">
                <span>Subtotal</span><span>{{ subtotal() | rdCurrency }}</span>
              </div>
              <div class="flex justify-between text-base-content/60">
                <span>ITBIS</span><span>{{ itbis() | rdCurrency }}</span>
              </div>
              <div class="flex justify-between text-lg font-black text-base-content">
                <span>Total</span><span>{{ total() | rdCurrency }}</span>
              </div>
            </div>

            <button appBtn class="w-full" (click)="onOpenCheckout()">
              <iconify-icon icon="lucide:hand-coins" class="text-base"></iconify-icon>
              Cobrar {{ total() | rdCurrency }}
            </button>
            <button class="tc-btn tc-btn-ghost tc-btn-sm w-full" (click)="clearCart()">Vaciar carrito</button>
          }
        </div>
      </div>
    }

    <!-- Abrir turno -->
    <app-modal #openShiftModal title="Abrir turno de caja">
      <form [formGroup]="openForm" (ngSubmit)="onOpenShift(openShiftModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="pos-cashier">
            <span class="text-xs font-bold uppercase tracking-wider">Cajero *</span>
          </label>
          <input id="pos-cashier" type="text" class="tc-input" formControlName="cashierName" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="pos-opening">
            <span class="text-xs font-bold uppercase tracking-wider">Efectivo inicial (RD$) *</span>
          </label>
          <input id="pos-opening" type="number" min="0" step="0.01" class="tc-input" formControlName="openingCashAmount" />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="openShiftModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="openForm.invalid">Abrir turno</button>
        </div>
      </form>
    </app-modal>

    <!-- Cerrar turno -->
    <app-modal #closeShiftModal title="Cerrar turno">
      @if (summary(); as s) {
        <div class="grid grid-cols-2 gap-2 text-sm mb-4">
          <div class="tc-card p-3"><p class="text-xs text-base-content/40 uppercase">Inicial</p><p class="font-black">{{ s.initialCash | rdCurrency }}</p></div>
          <div class="tc-card p-3"><p class="text-xs text-base-content/40 uppercase">Ventas efectivo</p><p class="font-black">{{ s.totalCashSales | rdCurrency }}</p></div>
          <div class="tc-card p-3"><p class="text-xs text-base-content/40 uppercase">Abonos</p><p class="font-black">{{ s.totalAccountPayments | rdCurrency }}</p></div>
          <div class="tc-card p-3"><p class="text-xs text-base-content/40 uppercase">Gastos</p><p class="font-black">{{ s.totalExpenses | rdCurrency }}</p></div>
          <div class="tc-card p-3 col-span-2 bg-primary/5"><p class="text-xs text-base-content/40 uppercase">Efectivo esperado</p><p class="font-black text-primary text-lg">{{ s.expectedCash | rdCurrency }}</p></div>
        </div>
      }
      <form [formGroup]="closeForm" (ngSubmit)="onCloseShift(closeShiftModal)" class="space-y-4">
        <div class="form-control">
          <label class="label pb-1" for="pos-actual">
            <span class="text-xs font-bold uppercase tracking-wider">Efectivo contado (RD$) *</span>
          </label>
          <input id="pos-actual" type="number" min="0" step="0.01" class="tc-input" formControlName="actualCashAmount" />
        </div>
        <div class="form-control">
          <label class="label pb-1" for="pos-notes">
            <span class="text-xs font-bold uppercase tracking-wider">Notas</span>
          </label>
          <input id="pos-notes" type="text" class="tc-input" formControlName="notes" placeholder="Opcional" />
        </div>
        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="closeShiftModal.close()">Cancelar</button>
          <button appBtn type="submit" [loading]="saving()" [disabled]="closeForm.invalid">Cerrar turno</button>
        </div>
      </form>
    </app-modal>

    <!-- Cobro -->
    <app-modal #checkoutModal title="Cobrar venta">
      <div class="space-y-4">
        <p class="text-3xl font-black text-primary text-center">{{ total() | rdCurrency }}</p>

        <div class="grid grid-cols-4 gap-1.5">
          @for (m of modes; track m.id) {
            <button
              type="button" class="tc-btn tc-btn-sm flex-col h-auto py-2"
              [class.tc-btn-primary]="mode() === m.id"
              (click)="mode.set(m.id)"
            >
              <iconify-icon [icon]="m.icon" class="text-lg"></iconify-icon>
              <span class="text-[10px]">{{ m.label }}</span>
            </button>
          }
        </div>

        @if (mode() === 'cash') {
          <div class="form-control">
            <label class="label pb-1" for="pos-tendered">
              <span class="text-xs font-bold uppercase tracking-wider">Recibido (RD$)</span>
            </label>
            <input
              id="pos-tendered" type="number" min="0" step="0.01" class="tc-input text-lg font-bold"
              [value]="tendered()" (input)="tendered.set(+$any($event.target).value || 0)"
            />
            @if (tendered() >= total()) {
              <p class="text-sm mt-2 text-base-content/60">Devuelta: <span class="font-black text-base-content">{{ tendered() - total() | rdCurrency }}</span></p>
            }
          </div>
        }

        @if (mode() === 'credit') {
          <div class="form-control">
            <label class="label pb-1" for="pos-customer">
              <span class="text-xs font-bold uppercase tracking-wider">Cliente (fiao) *</span>
            </label>
            <select id="pos-customer" class="tc-input" [value]="customerId() ?? ''" (change)="customerId.set($any($event.target).value || null)">
              <option value="">— Selecciona un cliente —</option>
              @for (c of customers(); track c.customerId) {
                <option [value]="c.customerId">{{ c.fullName }} (debe {{ c.balance | rdCurrency }})</option>
              }
            </select>
          </div>
        }

        @if (mode() === 'card' || mode() === 'transfer') {
          <div class="form-control">
            <label class="label pb-1" for="pos-ref">
              <span class="text-xs font-bold uppercase tracking-wider">Referencia</span>
            </label>
            <input id="pos-ref" type="text" class="tc-input" [value]="reference()" (input)="reference.set($any($event.target).value)" placeholder="Núm. de aprobación / transferencia" />
          </div>
        }

        @if (checkoutError(); as err) {
          <p class="text-sm text-secondary font-bold">{{ err }}</p>
        }

        <div modalActions>
          <button type="button" class="tc-btn tc-btn-ghost" (click)="checkoutModal.close()">Cancelar</button>
          <button appBtn [loading]="saving()" (click)="onConfirmSale(checkoutModal, receiptModal)">Confirmar venta</button>
        </div>
      </div>
    </app-modal>

    <!-- Recibo -->
    <app-modal #receiptModal title="Venta completada">
      @if (lastSale(); as r) {
        <div class="space-y-3">
          <div class="text-center">
            <iconify-icon icon="lucide:check-circle-2" class="text-5xl text-success"></iconify-icon>
            <p class="font-black text-lg mt-1">Recibo {{ r.receiptNumber }}</p>
            @if (r.ncfNumber) {
              <p class="text-xs text-base-content/40 uppercase tracking-wider">NCF {{ r.ncfNumber }}</p>
            }
          </div>
          <div class="space-y-1 text-sm border-t border-base-300 pt-2">
            @for (it of r.items; track it.productId) {
              <div class="flex justify-between">
                <span class="text-base-content/60">{{ it.quantity }} × {{ it.productName }}</span>
                <span class="font-bold">{{ it.lineTotal | rdCurrency }}</span>
              </div>
            }
          </div>
          <div class="border-t border-base-300 pt-2 text-sm space-y-1">
            <div class="flex justify-between text-base-content/60"><span>Subtotal</span><span>{{ r.subtotal | rdCurrency }}</span></div>
            <div class="flex justify-between text-base-content/60"><span>ITBIS</span><span>{{ r.totalItbis | rdCurrency }}</span></div>
            <div class="flex justify-between font-black text-lg"><span>Total</span><span>{{ r.total | rdCurrency }}</span></div>
            @if (r.changeDue > 0) {
              <div class="flex justify-between text-primary font-black"><span>Devuelta</span><span>{{ r.changeDue | rdCurrency }}</span></div>
            }
          </div>
          <div modalActions>
            <button appBtn (click)="receiptModal.close()">Nueva venta</button>
          </div>
        </div>
      }
    </app-modal>

    <!-- Resultado cierre -->
    <app-modal #closeResultModal title="Turno cerrado">
      @if (closeResult(); as r) {
        <div class="space-y-2 text-sm">
          <div class="flex justify-between"><span class="text-base-content/60">Ventas</span><span class="font-bold">{{ r.totalSalesCount }} ({{ r.totalSalesAmount | rdCurrency }})</span></div>
          <div class="flex justify-between"><span class="text-base-content/60">Esperado</span><span class="font-bold">{{ r.expectedCashAmount | rdCurrency }}</span></div>
          <div class="flex justify-between"><span class="text-base-content/60">Contado</span><span class="font-bold">{{ r.actualCashAmount | rdCurrency }}</span></div>
          <div class="flex justify-between text-lg font-black" [class.text-secondary]="r.cashDifference < 0">
            <span>Diferencia</span><span>{{ r.cashDifference | rdCurrency }}</span>
          </div>
          <div modalActions>
            <button appBtn (click)="closeResultModal.close()">Entendido</button>
          </div>
        </div>
      }
    </app-modal>
  `,
})
export class PosPage {
  private svc = inject(PosService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  shift = signal<Shift | null>(null);
  catalog = signal<CatalogItem[]>([]);
  summary = signal<ShiftSummary | null>(null);
  closeResult = signal<CloseShiftResult | null>(null);
  lastSale = signal<CreateSaleResult | null>(null);

  search = signal('');
  categoria = signal<string | null>(null);
  cart = signal<CartLine[]>([]);

  mode = signal<PaymentMode>('cash');
  tendered = signal(0);
  reference = signal('');
  customerId = signal<string | null>(null);
  customers = signal<{ customerId: string; fullName: string; balance: number; creditLimit: number; isActive: boolean }[]>([]);
  checkoutError = signal<string | null>(null);

  modes: { id: PaymentMode; label: string; icon: string }[] = [
    { id: 'cash', label: 'Efectivo', icon: 'lucide:banknote' },
    { id: 'card', label: 'Tarjeta', icon: 'lucide:credit-card' },
    { id: 'transfer', label: 'Transfer.', icon: 'lucide:smartphone' },
    { id: 'credit', label: 'Fiao', icon: 'lucide:notebook-pen' },
  ];

  categorias = computed(() => {
    const seen = new Map<string, string>();
    for (const i of this.catalog()) seen.set(i.categoryId, i.categoryName);
    return [...seen].map(([id, name]) => ({ id, name }));
  });

  filteredItems = computed(() => {
    const q = this.search().trim().toLowerCase();
    const cat = this.categoria();
    return this.catalog()
      .filter(i => i.isActive && (!cat || i.categoryId === cat))
      .map(i => ({
        ...i,
        presentations: i.presentations.filter(p =>
          p.isActive && (!q || p.displayName.toLowerCase().includes(q) || i.name.toLowerCase().includes(q))),
      }))
      .filter(i => i.presentations.length > 0);
  });

  subtotal = computed(() =>
    this.cart().reduce((acc, l) => {
      const line = l.presentation.salePrice * l.quantity;
      return acc + line / (1 + l.product.itbisRate);
    }, 0));
  total = computed(() => this.cart().reduce((acc, l) => acc + l.presentation.salePrice * l.quantity, 0));
  itbis = computed(() => this.total() - this.subtotal());

  openForm = this.fb.nonNullable.group({
    cashierName: ['', [Validators.required, Validators.minLength(2)]],
    openingCashAmount: [0, [Validators.required, Validators.min(0)]],
  });

  closeForm = this.fb.nonNullable.group({
    actualCashAmount: [0, [Validators.required, Validators.min(0)]],
    notes: [''],
  });

  constructor() {
    const u = this.auth.currentUser();
    const name = [u?.firstName, u?.lastName].filter(Boolean).join(' ') || u?.email || '';
    this.openForm.patchValue({ cashierName: name });
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.svc.getCurrentShift().subscribe({
      next: (s) => {
        this.shift.set(s);
        if (s) this.loadCatalog();
        else this.loading.set(false);
      },
      error: () => { this.shift.set(null); this.loading.set(false); },
    });
  }

  loadCatalog(): void {
    this.svc.getCatalog().subscribe({
      next: (items) => { this.catalog.set(items); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando el catálogo'); this.loading.set(false); },
    });
  }

  addToCart(product: CatalogItem, presentation: CatalogPresentation): void {
    this.cart.update(cart => {
      const existing = cart.find(l => l.presentation.id === presentation.id);
      if (existing) {
        return cart.map(l => l.presentation.id === presentation.id ? { ...l, quantity: l.quantity + 1 } : l);
      }
      return [...cart, { product, presentation, quantity: 1 }];
    });
  }

  setQty(line: CartLine, qty: number): void {
    if (qty <= 0) { this.removeLine(line); return; }
    this.cart.update(cart => cart.map(l => l.presentation.id === line.presentation.id ? { ...l, quantity: qty } : l));
  }

  removeLine(line: CartLine): void {
    this.cart.update(cart => cart.filter(l => l.presentation.id !== line.presentation.id));
  }

  clearCart(): void { this.cart.set([]); }

  onOpenShift(modal: ModalComponent): void {
    if (this.openForm.invalid) return;
    this.saving.set(true);
    const v = this.openForm.getRawValue();
    this.svc.openShift(v.openingCashAmount, v.cashierName).subscribe({
      next: () => { this.saving.set(false); modal.close(); this.toast.success('Turno abierto'); this.refresh(); },
      error: (e) => { this.saving.set(false); this.toast.error(e?.error?.detail ?? 'Error al abrir el turno'); },
    });
  }

  private closeShiftModal = viewChild<ModalComponent>('closeShiftModal');
  private checkoutModal = viewChild<ModalComponent>('checkoutModal');
  private closeResultModal = viewChild<ModalComponent>('closeResultModal');

  onOpenCloseModal(): void {
    this.svc.getCurrentShiftSummary().subscribe({
      next: (s) => {
        this.summary.set(s);
        this.closeForm.patchValue({ actualCashAmount: s.expectedCash, notes: '' });
      },
      error: () => this.summary.set(null),
    });
    this.closeShiftModal()?.open();
  }

  onCloseShift(modal: ModalComponent): void {
    const shift = this.shift();
    if (!shift || this.closeForm.invalid) return;
    this.saving.set(true);
    const v = this.closeForm.getRawValue();
    this.svc.closeShift(shift.shiftId, v.actualCashAmount, v.notes || null).subscribe({
      next: (r) => {
        this.saving.set(false);
        modal.close();
        this.closeResult.set(r);
        this.shift.set(null);
        this.cart.set([]);
        this.toast.success('Turno cerrado');
        this.closeResultModal()?.open();
      },
      error: (e) => { this.saving.set(false); this.toast.error(e?.error?.detail ?? 'Error al cerrar el turno'); },
    });
  }

  onOpenCheckout(): void {
    this.checkoutError.set(null);
    this.mode.set('cash');
    this.tendered.set(this.total());
    this.reference.set('');
    this.customerId.set(null);
    if (this.customers().length === 0) {
      this.svc.getCustomers().subscribe({ next: (c) => this.customers.set(c.filter(x => x.isActive)), error: () => {} });
    }
    this.checkoutModal()?.open();
  }

  onConfirmSale(checkout: ModalComponent, receipt: ModalComponent): void {
    const total = this.total();
    const mode = this.mode();
    this.checkoutError.set(null);

    if (mode === 'credit' && !this.customerId()) {
      this.checkoutError.set('Selecciona el cliente para la venta fiao.');
      return;
    }
    if (mode === 'cash' && this.tendered() < total) {
      this.checkoutError.set('El monto recibido es menor al total.');
      return;
    }

    const methodId = mode === 'cash' ? PAYMENT_METHODS.CASH
      : mode === 'card' ? PAYMENT_METHODS.CARD
      : mode === 'transfer' ? PAYMENT_METHODS.TRANSFER
      : PAYMENT_METHODS.CREDIT;

    const items = this.cart().map(l => ({
      productId: l.product.productId,
      presentationId: l.presentation.id,
      quantity: l.quantity,
    }));
    const payments = [{
      paymentMethodId: methodId,
      amount: mode === 'cash' ? this.tendered() : total,
      reference: this.reference() || null,
      customerId: mode === 'credit' ? this.customerId() : null,
    }];

    this.saving.set(true);
    this.svc.createSale(items, payments, null, null).subscribe({
      next: (r) => {
        this.saving.set(false);
        checkout.close();
        this.lastSale.set(r);
        this.cart.set([]);
        receipt.open();
      },
      error: (e) => {
        this.saving.set(false);
        this.checkoutError.set(e?.error?.detail ?? 'No se pudo registrar la venta.');
      },
    });
  }
}
