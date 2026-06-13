import {
  Component, ChangeDetectionStrategy, inject, signal, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { DeliveriesService } from '../portal/deliveries/services/deliveries.service';
import { DeliveryPendiente, direccionCompleta } from '../portal/deliveries/models/delivery.model';
import { SpinnerComponent } from '../shared/ui/spinner/spinner.component';
import { BadgeComponent } from '../shared/ui/badge/badge.component';
import { ModalComponent } from '../shared/ui/modal/modal.component';
import { BtnComponent } from '../shared/ui/btn/btn.component';
import { ToastService } from '../shared/ui/toast/toast.service';
import { RdCurrencyPipe } from '../shared/ui/pipes/rd-currency.pipe';

const PM_CASH = 1;

@Component({
  selector: 'app-delivery-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [DatePipe, SpinnerComponent, BadgeComponent, ModalComponent, BtnComponent, RdCurrencyPipe],
  template: `
    <div class="min-h-screen bg-base-200 p-4 space-y-4 max-w-2xl mx-auto">
      <div class="flex items-center justify-between">
        <h1 class="text-2xl font-black text-base-content tracking-tight">
          <iconify-icon icon="lucide:bike" class="text-primary align-middle mr-1"></iconify-icon>
          Entregas
        </h1>
        <button appBtn variant="ghost" size="sm" (click)="load()">
          <iconify-icon icon="lucide:refresh-cw"></iconify-icon>
          Actualizar
        </button>
      </div>

      @if (loading()) {
        <div class="flex justify-center py-16"><app-spinner size="lg" /></div>
      } @else if (pendientes().length === 0) {
        <div class="flex flex-col items-center py-20 gap-3">
          <iconify-icon icon="lucide:coffee" class="text-6xl text-base-content/20"></iconify-icon>
          <p class="text-base-content/40">No hay entregas pendientes</p>
        </div>
      } @else {
        @for (d of pendientes(); track d.id) {
          <div class="tc-card p-4 space-y-3">
            <div class="flex items-start justify-between gap-2">
              <div>
                <p class="font-black">{{ d.customerName || 'Sin nombre' }}</p>
                <p class="text-xs text-base-content/40 font-mono">{{ d.receiptNumber }} · {{ d.createdAt | date:'HH:mm' }}</p>
              </div>
              <app-badge [variant]="d.status === 'InTransit' ? 'warning' : 'ghost'">{{ d.status }}</app-badge>
            </div>

            <div class="text-sm space-y-1">
              <p><iconify-icon icon="lucide:map-pin" class="text-primary align-middle mr-1"></iconify-icon>{{ direccion(d) }}</p>
              @if (d.addressReference) {
                <p class="text-base-content/50 text-xs pl-5">{{ d.addressReference }}</p>
              }
              @if (d.phone) {
                <a class="text-primary font-bold" href="tel:{{ d.phone }}">
                  <iconify-icon icon="lucide:phone" class="align-middle mr-1"></iconify-icon>{{ d.phone }}
                </a>
              }
            </div>

            <div class="flex items-center justify-between pt-1">
              <p class="text-lg font-black text-primary">{{ d.totalAmount | rdCurrency }}</p>
              @if (d.status === 'InTransit') {
                <button appBtn size="sm" (click)="onStartComplete(d, completeModal)">
                  <iconify-icon icon="lucide:check"></iconify-icon>
                  Entregada
                </button>
              } @else {
                <button appBtn size="sm" [loading]="working() === d.id" (click)="onAccept(d)">
                  <iconify-icon icon="lucide:navigation"></iconify-icon>
                  Aceptar
                </button>
              }
            </div>
          </div>
        }
      }
    </div>

    <!-- Completar entrega -->
    <app-modal #completeModal title="Confirmar entrega">
      @if (seleccionada(); as d) {
        <div class="space-y-4">
          <p class="text-sm text-base-content/60">
            Cobra <span class="font-black text-base-content">{{ d.totalAmount | rdCurrency }}</span>
            y pide al cliente su código de confirmación.
          </p>
          <div class="form-control">
            <label class="label pb-1" for="dlv-code">
              <span class="text-xs font-bold uppercase tracking-wider">Código de confirmación *</span>
            </label>
            <input
              id="dlv-code" type="text" class="tc-input text-center text-xl font-black tracking-[0.3em]"
              [value]="codigo()" (input)="codigo.set($any($event.target).value)" maxlength="8" autocomplete="off"
            />
          </div>
          @if (error(); as err) {
            <p class="text-sm text-secondary font-bold">{{ err }}</p>
          }
          <div modalActions>
            <button type="button" class="tc-btn tc-btn-ghost" (click)="completeModal.close()">Cancelar</button>
            <button appBtn [loading]="working() === d.id" [disabled]="!codigo().trim()" (click)="onComplete(d, completeModal)">
              Confirmar entrega
            </button>
          </div>
        </div>
      }
    </app-modal>
  `,
})
export class DeliveryPage {
  private svc = inject(DeliveriesService);
  private toast = inject(ToastService);

  loading = signal(true);
  working = signal<string | null>(null);
  pendientes = signal<DeliveryPendiente[]>([]);
  seleccionada = signal<DeliveryPendiente | null>(null);
  codigo = signal('');
  error = signal<string | null>(null);

  constructor() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getPendientes().subscribe({
      next: (items) => { this.pendientes.set(items); this.loading.set(false); },
      error: () => { this.toast.error('Error cargando entregas'); this.loading.set(false); },
    });
  }

  direccion(d: DeliveryPendiente): string { return direccionCompleta(d); }

  onAccept(d: DeliveryPendiente): void {
    this.working.set(d.id);
    this.svc.aceptar(d.id).subscribe({
      next: () => { this.working.set(null); this.toast.success('Entrega aceptada'); this.load(); },
      error: (e) => { this.working.set(null); this.toast.error(e?.error?.detail ?? 'No se pudo aceptar'); },
    });
  }

  onStartComplete(d: DeliveryPendiente, modal: ModalComponent): void {
    this.seleccionada.set(d);
    this.codigo.set('');
    this.error.set(null);
    modal.open();
  }

  onComplete(d: DeliveryPendiente, modal: ModalComponent): void {
    this.working.set(d.id);
    this.error.set(null);
    const payments = [{ paymentMethodId: PM_CASH, amount: d.totalAmount, reference: null, customerId: null }];
    this.svc.completar(d.id, payments, this.codigo().trim()).subscribe({
      next: () => {
        this.working.set(null);
        modal.close();
        this.toast.success('Entrega completada');
        this.load();
      },
      error: (e) => {
        this.working.set(null);
        this.error.set(e?.error?.detail ?? 'Código incorrecto o error al completar.');
      },
    });
  }
}
