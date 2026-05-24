import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Empleado } from '../models/empleado.model';

@Injectable({ providedIn: 'root' })
export class EmpleadosService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/auth/employees`;

  getEmpleados(): Observable<Empleado[]> {
    return this.http.get<Empleado[]>(this.api);
  }

  createEmpleado(data: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role: string;
  }): Observable<Empleado> {
    return this.http.post<Empleado>(this.api, data);
  }

  updateEmpleado(id: string, data: Partial<{ role: string; isActive: boolean }>): Observable<Empleado> {
    return this.http.patch<Empleado>(`${this.api}/${id}`, data);
  }
}
