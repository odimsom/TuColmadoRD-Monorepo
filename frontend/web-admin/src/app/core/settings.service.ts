import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BusinessProfile {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway`;

  getProfile(): Observable<BusinessProfile | null> {
    return this.http.get<BusinessProfile | null>(`${this.api}/settings/profile`);
  }
}
