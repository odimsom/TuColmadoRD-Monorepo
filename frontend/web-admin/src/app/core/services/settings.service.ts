import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { API_PATHS } from '../constants';

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
    return this.gateway.get<TenantProfileDto | null>(API_PATHS.SETTINGS_PROFILE);
  }

  upsertProfile(req: UpsertTenantProfileRequest): Observable<void> {
    return this.gateway.put<void>(API_PATHS.SETTINGS_PROFILE, req);
  }
}
