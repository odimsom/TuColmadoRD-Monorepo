import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { FondoMonetario, PagedTransacciones } from '../models/caja.model';

@Injectable({ providedIn: 'root' })
export class CajasService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1/inventory`;

  getFondos(): Observable<FondoMonetario[]> {
    return this.http.get<FondoMonetario[]>(`${this.api}/funds`);
  }

  getFondo(id: string): Observable<FondoMonetario> {
    return this.http.get<FondoMonetario>(`${this.api}/funds/${id}`);
  }

  createFondo(name: string, initialDeposit: number): Observable<{ fundId: string }> {
    return this.http.post<{ fundId: string }>(`${this.api}/funds`, { name, initialDeposit });
  }

  getTransacciones(fundId: string, page = 1, pageSize = 20): Observable<PagedTransacciones> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedTransacciones>(`${this.api}/funds/${fundId}/transactions`, { params });
  }

  depositar(fundId: string, amount: number, description: string): Observable<{ transactionId: string }> {
    return this.http.post<{ transactionId: string }>(`${this.api}/funds/${fundId}/deposit`, { amount, description });
  }

  registrarGasto(fundId: string, amount: number, category: string, description: string): Observable<{ transactionId: string }> {
    return this.http.post<{ transactionId: string }>(`${this.api}/funds/${fundId}/expense`, {
      amount,
      category,
      description,
    });
  }
}
