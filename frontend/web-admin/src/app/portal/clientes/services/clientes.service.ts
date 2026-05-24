import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Cliente, ClienteEstadoCuenta, PagedClientes } from '../models/cliente.model';

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1/customers`;

  getClientes(page = 1, pageSize = 20): Observable<PagedClientes> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedClientes>(this.api, { params });
  }

  getCliente(id: string): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.api}/${id}`);
  }

  createCliente(data: {
    fullName: string;
    documentId: string;
    phone?: string | null;
    creditLimit?: number | null;
  }): Observable<{ customerId: string }> {
    return this.http.post<{ customerId: string }>(this.api, data);
  }

  getEstadoCuenta(id: string): Observable<ClienteEstadoCuenta[]> {
    return this.http.get<ClienteEstadoCuenta[]>(`${this.api}/${id}/statement`);
  }

  registrarPago(id: string, amount: number, paymentMethodId: number, concept: string): Observable<void> {
    return this.http.post<void>(`${this.api}/${id}/payments`, { amount, paymentMethodId, concept });
  }
}
