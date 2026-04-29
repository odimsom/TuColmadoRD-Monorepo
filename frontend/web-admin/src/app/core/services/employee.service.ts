import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  private http = inject(HttpClient);
  private base = `${environment.gatewayUrl}/gateway/auth/employees`;

  list(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(this.base);
  }

  create(data: CreateEmployeeDto): Observable<EmployeeDto> {
    return this.http.post<EmployeeDto>(this.base, data);
  }

  update(id: string, data: UpdateEmployeeDto): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(`${this.base}/${id}`, data);
  }

  toggle(id: string, active: boolean): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}`, { active });
  }
}
