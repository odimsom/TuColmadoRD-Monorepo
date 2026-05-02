import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';

export interface ProductDto {
  productId: string;
  name: string;
  categoryId: string;
  categoryName: string;
  costPrice: number;
  salePrice: number;
  itbisRate: number;
  unitTypeId: number;
  unitTypeName: string;
  stockQuantity: number;
  isActive: boolean;
  updatedAt: string;
}

export interface PagedProductsResponse {
  items: ProductDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateProductRequest {
  name: string;
  categoryId: string;
  costPrice: number;
  salePrice: number;
  itbisRate: number;
  unitType: number;
}

export interface UpdatePriceRequest {
  newCostPrice: number;
  newSalePrice: number;
}

export interface AdjustStockRequest {
  delta: number;
  reason: string;
}

export interface CategoryDto {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private gateway = inject(GatewayService);

  getProducts(page = 1, pageSize = 20, nameFilter?: string, categoryId?: string): Observable<PagedProductsResponse> {
    const params: any = { page, pageSize };
    if (nameFilter) params['nameFilter'] = nameFilter;
    if (categoryId) params['categoryId'] = categoryId;
    return this.gateway.get<PagedProductsResponse>('/api/v1/inventory/products', params);
  }

  getProductById(id: string): Observable<ProductDto> {
    return this.gateway.get<ProductDto>(`/api/v1/inventory/products/${id}`);
  }

  createProduct(cmd: CreateProductRequest): Observable<{ productId: string }> {
    return this.gateway.post('/api/v1/inventory/products', cmd);
  }

  updatePrice(id: string, req: UpdatePriceRequest): Observable<void> {
    return this.gateway.put(`/api/v1/inventory/products/${id}/price`, req);
  }

  adjustStock(id: string, req: AdjustStockRequest): Observable<{ newStockQuantity: number }> {
    return this.gateway.post(`/api/v1/inventory/products/${id}/stock/adjust`, req);
  }

  deactivateProduct(id: string): Observable<void> {
    return this.gateway.delete(`/api/v1/inventory/products/${id}`);
  }

  getCatalog(): Observable<ProductDto[]> {
    return this.gateway.get<ProductDto[]>('/api/v1/inventory/catalog');
  }

  getLowStock(threshold = 5): Observable<{ count: number; items: { productId: string; name: string; stockQuantity: number }[] }> {
    return this.gateway.get('/api/v1/inventory/products/low-stock', { threshold });
  }

  getCategories(): Observable<CategoryDto[]> {
    return this.gateway.get<CategoryDto[]>('/api/v1/inventory/categories');
  }

  createCategory(name: string): Observable<{ id: string }> {
    return this.gateway.post<{ id: string }>('/api/v1/inventory/categories', { name });
  }

  seedDefaultCategories(): Observable<{ created: number }> {
    return this.gateway.post<{ created: number }>('/api/v1/inventory/categories/seed-defaults', {});
  }

  deactivateCategory(id: string): Observable<void> {
    return this.gateway.delete(`/api/v1/inventory/categories/${id}`);
  }
}
