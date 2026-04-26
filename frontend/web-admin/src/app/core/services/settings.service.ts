import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';

export interface TenantProfileDto {
  businessName: string;
  rnc: string | null;
  businessAddress: string;
  phone: string | null;
  email: string | null;
}

export interface UpsertTenantProfileRequest {
  businessName: string;
  rnc: string | null;
  businessAddress: string;
  phone: string | null;
  email: string | null;
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private gateway = inject(GatewayService);

  getProfile(): Observable<TenantProfileDto | null> {
    return this.gateway.get<TenantProfileDto | null>('/api/v1/settings/profile');
  }

  upsertProfile(req: UpsertTenantProfileRequest): Observable<void> {
    return this.gateway.put<void>('/api/v1/settings/profile', req);
  }
}
