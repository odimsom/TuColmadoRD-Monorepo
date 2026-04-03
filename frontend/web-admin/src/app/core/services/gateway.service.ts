import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GatewayService {
  private http = inject(HttpClient);
  private base = `${environment.gatewayUrl}/gateway`;

  get<T>(path: string, params?: any): Observable<T> {
    const httpParams = this.formatParams(params);
    return this.http.get<T>(`${this.base}${this.cleanPath(path)}`, { params: httpParams });
  }

  post<T>(path: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.base}${this.cleanPath(path)}`, body);
  }

  put<T>(path: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.base}${this.cleanPath(path)}`, body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.base}${this.cleanPath(path)}`);
  }

  private cleanPath(path: string): string {
    return path.startsWith('/') ? path : `/${path}`;
  }

  private formatParams(params: any): HttpParams {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key]);
        }
      });
    }
    return httpParams;
  }
}
