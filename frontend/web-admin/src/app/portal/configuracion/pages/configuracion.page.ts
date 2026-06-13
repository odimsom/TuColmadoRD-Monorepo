import {
  Component, ChangeDetectionStrategy, inject, signal, OnInit, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CardComponent } from '../../../shared/ui/card/card.component';
import { SpinnerComponent } from '../../../shared/ui/spinner/spinner.component';
import { BtnComponent } from '../../../shared/ui/btn/btn.component';
import { ToastService } from '../../../shared/ui/toast/toast.service';
import { environment } from '../../../../environments/environment';

interface TenantProfile {
  businessName: string;
  rnc: string | null;
  businessAddress: string;
  phone: string | null;
  email: string | null;
}

@Component({
  selector: 'app-configuracion',
  changeDetection: ChangeDetectionStrategy.OnPush,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [ReactiveFormsModule, CardComponent, SpinnerComponent, BtnComponent],
  template: `
    <div class="space-y-5">

      <!-- Header -->
      <div>
        <h2 class="text-2xl font-black text-base-content tracking-tight">Configuración</h2>
        <p class="text-sm text-base-content/50 mt-1">Perfil del negocio y datos fiscales</p>
      </div>

      @if (loading()) {
        <div class="flex justify-center py-16">
          <app-spinner size="lg" />
        </div>
      } @else {

        <app-card>
          <form [formGroup]="profileForm" (ngSubmit)="onSave()" class="space-y-5">

            <div class="form-control">
              <label class="label pb-1" for="cfg-name">
                <span class="text-xs font-bold text-base-content uppercase tracking-wider">Nombre del negocio *</span>
              </label>
              <input
                id="cfg-name"
                type="text"
                class="tc-input"
                formControlName="businessName"
                placeholder="Tu Colmado RD..."
              />
            </div>

            <div class="form-control">
              <label class="label pb-1" for="cfg-address">
                <span class="text-xs font-bold text-base-content uppercase tracking-wider">Dirección *</span>
              </label>
              <input
                id="cfg-address"
                type="text"
                class="tc-input"
                formControlName="businessAddress"
                placeholder="Calle principal #123, Santo Domingo..."
              />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
              <div class="form-control">
                <label class="label pb-1" for="cfg-rnc">
                  <span class="text-xs font-bold text-base-content uppercase tracking-wider">RNC</span>
                  <span class="text-[10px] text-base-content/40 uppercase font-bold">Opcional</span>
                </label>
                <input
                  id="cfg-rnc"
                  type="text"
                  class="tc-input"
                  formControlName="rnc"
                  placeholder="1-31-00000-0"
                />
              </div>
              <div class="form-control">
                <label class="label pb-1" for="cfg-phone">
                  <span class="text-xs font-bold text-base-content uppercase tracking-wider">Teléfono</span>
                  <span class="text-[10px] text-base-content/40 uppercase font-bold">Opcional</span>
                </label>
                <input
                  id="cfg-phone"
                  type="tel"
                  class="tc-input"
                  formControlName="phone"
                  placeholder="809-000-0000"
                />
              </div>
            </div>

            <div class="form-control">
              <label class="label pb-1" for="cfg-email">
                <span class="text-xs font-bold text-base-content uppercase tracking-wider">Correo del negocio</span>
                <span class="text-[10px] text-base-content/40 uppercase font-bold">Opcional</span>
              </label>
              <input
                id="cfg-email"
                type="email"
                class="tc-input"
                formControlName="email"
                placeholder="info@tunegocio.com"
              />
            </div>

            <div class="flex justify-end pt-2">
              <button appBtn type="submit" [loading]="saving()" [disabled]="profileForm.invalid || profileForm.pristine">
                <iconify-icon icon="lucide:save" class="text-base"></iconify-icon>
                Guardar cambios
              </button>
            </div>
          </form>
        </app-card>
      }
    </div>
  `,
})
export class ConfiguracionPage implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);

  private api = `${environment.gatewayUrl}/gateway/api/v1/settings/profile`;

  loading = signal(true);
  saving = signal(false);

  profileForm = this.fb.nonNullable.group({
    businessName: ['', [Validators.required, Validators.minLength(2)]],
    businessAddress: ['', Validators.required],
    rnc: [''],
    phone: [''],
    email: ['', Validators.email],
  });

  constructor() { this.loadProfile(); }

  ngOnInit(): void { /* implemented via constructor */ }

  loadProfile(): void {
    this.loading.set(true);
    this.http.get<TenantProfile | null>(this.api).subscribe({
      next: (p) => {
        if (p) {
          this.profileForm.patchValue({
            businessName: p.businessName,
            businessAddress: p.businessAddress,
            rnc: p.rnc ?? '',
            phone: p.phone ?? '',
            email: p.email ?? '',
          });
          this.profileForm.markAsPristine();
        }
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  onSave(): void {
    if (this.profileForm.invalid) return;
    this.saving.set(true);
    const val = this.profileForm.getRawValue();
    this.http.put(this.api, {
      businessName: val.businessName,
      businessAddress: val.businessAddress,
      rnc: val.rnc || null,
      phone: val.phone || null,
      email: val.email || null,
    }).subscribe({
      next: () => {
        this.toast.success('Configuración guardada');
        this.profileForm.markAsPristine();
        this.saving.set(false);
      },
      error: () => { this.toast.error('Error al guardar'); this.saving.set(false); },
    });
  }
}
