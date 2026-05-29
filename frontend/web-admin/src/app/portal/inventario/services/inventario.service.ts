import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Categoria, PagedProductos, Presentacion, Producto } from '../models/producto.model';

@Injectable({ providedIn: 'root' })
export class InventarioService {
  private http = inject(HttpClient);
  private api = `${environment.gatewayUrl}/gateway/api/v1/inventory`;

  getProductos(page = 1, pageSize = 20, nameFilter?: string, categoryId?: string): Observable<PagedProductos> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (nameFilter) params = params.set('nameFilter', nameFilter);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<PagedProductos>(`${this.api}/products`, { params });
  }

  getProducto(id: string): Observable<Producto> {
    return this.http.get<Producto>(`${this.api}/products/${id}`);
  }

  createProducto(name: string, categoryId: string, itbisRate: number): Observable<{ productId: string }> {
    return this.http.post<{ productId: string }>(`${this.api}/products`, { name, categoryId, itbisRate });
  }

  deactivateProducto(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/products/${id}`);
  }

  getCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${this.api}/categories`);
  }

  createCategoria(name: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.api}/categories`, { name });
  }

  getPresentaciones(productId: string): Observable<Presentacion[]> {
    return this.http.get<Presentacion[]>(`${this.api}/products/${productId}/presentations`);
  }

  addPresentacion(
    productId: string,
    data: {
      displayName: string;
      presentationType: number;
      sellMode: number;
      measureUnit: number;
      salePrice: number;
      costPrice: number;
      brand?: string | null;
      nominalCapacity?: number | null;
    },
  ): Observable<{ presentationId: string }> {
    return this.http.post<{ presentationId: string }>(`${this.api}/products/${productId}/presentations`, data);
  }

  updatePresentacionPrice(id: string, salePrice: number, costPrice: number): Observable<void> {
    return this.http.put<void>(`${this.api}/presentations/${id}/price`, { salePrice, costPrice });
  }

  deactivatePresentacion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/presentations/${id}`);
  }

  getCatalogo(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/catalog`);
  }
}
