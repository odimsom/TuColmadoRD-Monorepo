import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import { API_PATHS } from '../constants';

export interface EmployeeDto {
  _id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  role: string;
  isActive: boolean;
  createdAt: string;
  tenantId: string;
}

export interface CreateEmployeeDto {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  role: string;
}

export interface UpdateEmployeeDto {
  firstName?: string;
  lastName?: string;
  role?: string;
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private gateway = inject(GatewayService);

  list(): Observable<EmployeeDto[]> {
    return this.gateway.get<EmployeeDto[]>(API_PATHS.AUTH_EMPLOYEES);
  }

  create(data: CreateEmployeeDto): Observable<EmployeeDto> {
    return this.gateway.post<EmployeeDto>(API_PATHS.AUTH_EMPLOYEES, data);
  }

  update(id: string, data: UpdateEmployeeDto): Observable<EmployeeDto> {
    return this.gateway.put<EmployeeDto>(`${API_PATHS.AUTH_EMPLOYEES}/${id}`, data);
  }

  toggle(id: string, active: boolean): Observable<void> {
    return this.gateway.patch<void>(`${API_PATHS.AUTH_EMPLOYEES}/${id}`, { active });
  }
}
