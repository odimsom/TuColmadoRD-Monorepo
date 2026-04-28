import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { SettingsService, TenantProfileDto } from '../../../core/services/settings.service';
import { RncPipe, RdPhonePipe } from '../../../core/pipes';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RncPipe, RdPhonePipe],
  templateUrl: './settings.html',
})
export class Settings implements OnInit {
  private settingsService = inject(SettingsService);
  private fb = inject(FormBuilder);

  loading = signal(true);
  saving = signal(false);
  successMsg = signal<string | null>(null);
  errorMsg = signal<string | null>(null);
  isNewProfile = signal(false);

  form = this.fb.group({
    businessName:    ['', [Validators.required, Validators.maxLength(200)]],
    rnc:             ['', [Validators.pattern(/^\d{9}$/)]],
    businessAddress: ['', [Validators.required, Validators.maxLength(500)]],
    phone:           [''],
    email:           ['', [Validators.email]],
  });

  ngOnInit(): void {
    this.settingsService.getProfile().subscribe({
      next: (profile) => {
        if (profile) {
          this.form.patchValue({
            businessName:    profile.businessName,
            rnc:             profile.rnc ?? '',
            businessAddress: profile.businessAddress,
            phone:           profile.phone ?? '',
            email:           profile.email ?? '',
          });
          this.isNewProfile.set(false);
        } else {
          this.isNewProfile.set(true);
        }
        this.loading.set(false);
      },
      error: () => {
        this.errorMsg.set('No se pudo cargar el perfil. Intente de nuevo.');
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (this.form.invalid || this.saving()) return;

    this.saving.set(true);
    this.successMsg.set(null);
    this.errorMsg.set(null);

    const v = this.form.value;
    this.settingsService.upsertProfile({
      businessName:    v.businessName!.trim(),
      rnc:             v.rnc?.trim() || null,
      businessAddress: v.businessAddress!.trim(),
      phone:           v.phone?.trim() || null,
      email:           v.email?.trim() || null,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.isNewProfile.set(false);
        this.successMsg.set('Perfil guardado correctamente.');
        setTimeout(() => this.successMsg.set(null), 4000);
      },
      error: (err) => {
        this.saving.set(false);
        const msg = err?.error?.message ?? err?.error?.detail ?? 'Error al guardar. Intente de nuevo.';
        this.errorMsg.set(msg);
      }
    });
  }

  get profileForReceipt(): TenantProfileDto {
    const v = this.form.value;
    return {
      businessName:    v.businessName || '—',
      rnc:             v.rnc || null,
      businessAddress: v.businessAddress || '—',
      phone:           v.phone || null,
      email:           v.email || null,
    };
  }
}
