import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CatalogItem, Shift, ShiftSummary, CloseShiftResult,
  CreateSalePayment, CreateSaleResult,
} from '../models/pos.model';

@Injectable({ providedIn: 'root' })
export class PosService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1`;

  getCatalog(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>(`${this.api}/inventory/catalog`);
  }

  getCurrentShift(): Observable<Shift | null> {
    return this.http.get<Shift | null>(`${this.api}/sales/shifts/current`);
  }

  getCurrentShiftSummary(): Observable<ShiftSummary> {
    return this.http.get<ShiftSummary>(`${this.api}/sales/shifts/current/summary`);
  }

  openShift(openingCashAmount: number, cashierName: string): Observable<{ shiftId: string }> {
    return this.http.post<{ shiftId: string }>(`${this.api}/sales/shifts/open`, {
      openingCashAmount, cashierName,
    });
  }

  closeShift(shiftId: string, actualCashAmount: number, notes: string | null): Observable<CloseShiftResult> {
    return this.http.post<CloseShiftResult>(`${this.api}/sales/shifts/${shiftId}/close`, {
      actualCashAmount, notes,
    });
  }

  // Clientes para ventas a crédito (fiao). La API devuelve la lista completa.
  getCustomers(): Observable<{ customerId: string; fullName: string; balance: number; creditLimit: number; isActive: boolean }[]> {
    return this.http.get<{ customerId: string; fullName: string; balance: number; creditLimit: number; isActive: boolean }[]>(
      `${this.api}/customers`,
    );
  }

  createSale(
    items: { productId: string; presentationId: string; quantity: number }[],
    payments: CreateSalePayment[],
    notes: string | null,
    buyerRnc: string | null,
  ): Observable<CreateSaleResult> {
    return this.http.post<CreateSaleResult>(`${this.api}/sales`, {
      items, payments, notes, buyerRnc,
    });
  }
}
