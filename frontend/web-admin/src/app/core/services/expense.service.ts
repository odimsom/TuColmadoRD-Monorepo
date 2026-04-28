import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';

export interface ExpenseSummary {
  id: string;
  amount: number;
  category: string;
  description: string;
  date: string;
}

export interface RegisterExpenseRequest {
  amount: number;
  category: string;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private gw = inject(GatewayService);

  getExpenses(page = 1, pageSize = 50): Observable<ExpenseSummary[]> {
    return this.gw.get<ExpenseSummary[]>('/api/v1/expenses', { page, pageSize });
  }

  registerExpense(req: RegisterExpenseRequest): Observable<void> {
    return this.gw.post<void>('/api/v1/expenses', req);
  }
}
