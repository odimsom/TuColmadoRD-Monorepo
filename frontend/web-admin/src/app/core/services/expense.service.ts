import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { API_PATHS } from '../constants';

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
    return this.gw.get<ExpenseSummary[]>(API_PATHS.EXPENSES, { page, pageSize });
  }

  registerExpense(req: RegisterExpenseRequest): Observable<void> {
    return this.gw.post<void>(API_PATHS.EXPENSES, req);
  }
}
