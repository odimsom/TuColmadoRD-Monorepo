import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BusinessProfile {
  businessName: string;
  rnc: string | null;
  businessAddress: string;
  phone: string | null;
  email: string | null;
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1`;

  getProfile(): Observable<BusinessProfile | null> {
    return this.http.get<BusinessProfile | null>(`${this.api}/settings/profile`);
  }
}
