import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { API_PATHS } from '../constants';

export interface CustomerSummary {
  customerId: string;
  fullName: string;
  phone: string;
  balance: number;
  creditLimit: number;
  isActive: boolean;
  province?: string | null;
  sector?: string | null;
  street?: string | null;
  houseNumber?: string | null;
  reference?: string | null;
  latitude?: number | null;
  longitude?: number | null;
}

export interface CustomerStatementEntry {
  transactionId: string;
  date: string;
  type: string;
  amount: number;
  concept: string;
}

export interface CreateCustomerAddressRequest {
  province: string;
  sector: string;
  street: string;
  reference: string;
  houseNumber?: string;
  latitude?: number;
  longitude?: number;
}

export interface CreateCustomerRequest {
  fullName: string;
  documentId: string;
  phone: string | null;
  address?: CreateCustomerAddressRequest | null;
  creditLimit: number | null;
}

export interface RegisterPaymentRequest {
  amount: number;
  paymentMethodId: number;
  concept: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private gateway = inject(GatewayService);

  getCustomers(): Observable<CustomerSummary[]> {
    return this.gateway.get<CustomerSummary[]>(API_PATHS.CUSTOMERS);
  }

  createCustomer(req: CreateCustomerRequest): Observable<{ customerId: string }> {
    return this.gateway.post(API_PATHS.CUSTOMERS, req);
  }

  registerPayment(customerId: string, req: RegisterPaymentRequest): Observable<void> {
    return this.gateway.post(API_PATHS.CUSTOMER_PAYMENTS(customerId), req);
  }

  getStatement(customerId: string): Observable<CustomerStatementEntry[]> {
    return this.gateway.get<CustomerStatementEntry[]>(API_PATHS.CUSTOMER_STATEMENT(customerId));
  }
}
