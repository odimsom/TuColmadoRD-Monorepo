import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { GatewayService } from './gateway.service';
import {
  ProductDto,
  PagedProductsResponse,
  CreateProductRequest,
  UpdatePresentationPriceRequest,
  StockEntryLineRequest,
  ConfirmStockEntryRequest,
  OpenContainerRequest,
  DrawFromContainerRequest,
  SetActiveContainerRequest,
  StockContainerDto,
  StockEntryDto,
  ProductPresentationDto,
  LowStockPresentationDto,
  OpenContainerSummaryDto,
  CategoryDto,
} from '../models/inventory.models';
export type { CategoryDto, ProductPresentationDto, StockContainerDto, StockEntryDto, ProductDto, PagedProductsResponse, CreateProductRequest, UpdatePresentationPriceRequest, StockEntryLineRequest, ConfirmStockEntryRequest, OpenContainerRequest, DrawFromContainerRequest, SetActiveContainerRequest, LowStockPresentationDto, OpenContainerSummaryDto } from '../models/inventory.models';
import { API_PATHS, LOW_STOCK_THRESHOLD } from '../constants';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private gateway = inject(GatewayService);

  getProducts(page = 1, pageSize = 20, nameFilter?: string, categoryId?: string): Observable<PagedProductsResponse> {
    const params: any = { page, pageSize };
    if (nameFilter) params['nameFilter'] = nameFilter;
    if (categoryId) params['categoryId'] = categoryId;
    return this.gateway.get<PagedProductsResponse>(API_PATHS.INVENTORY_PRODUCTS, params);
  }

  getProductById(id: string): Observable<ProductDto> {
    return this.gateway.get<ProductDto>(`${API_PATHS.INVENTORY_PRODUCTS}/${id}`);
  }

  createProduct(cmd: CreateProductRequest): Observable<{ productId: string }> {
    return this.gateway.post(API_PATHS.INVENTORY_PRODUCTS, cmd);
  }

  updatePresentationPrice(presentationId: string, req: UpdatePresentationPriceRequest): Observable<void> {
    return this.gateway.put(`/api/v1/inventory/presentations/${presentationId}/price`, req);
  }

  deactivateProduct(id: string): Observable<void> {
    return this.gateway.delete(`${API_PATHS.INVENTORY_PRODUCTS}/${id}`);
  }

  getCatalog(): Observable<ProductDto[]> {
    return this.gateway.get<ProductDto[]>(API_PATHS.INVENTORY_CATALOG);
  }

  getLowStockPresentations(threshold = LOW_STOCK_THRESHOLD): Observable<{ count: number; items: LowStockPresentationDto[] }> {
    return this.gateway.get(API_PATHS.INVENTORY_LOW_STOCK, { threshold });
  }

  getCategories(): Observable<CategoryDto[]> {
    return this.gateway.get<CategoryDto[]>(API_PATHS.INVENTORY_CATEGORIES);
  }

  createCategory(name: string): Observable<{ id: string }> {
    return this.gateway.post<{ id: string }>(API_PATHS.INVENTORY_CATEGORIES, { name });
  }

  seedDefaultCategories(): Observable<{ created: number }> {
    return this.gateway.post<{ created: number }>(`${API_PATHS.INVENTORY_CATEGORIES}/seed-defaults`, {});
  }

  deactivateCategory(id: string): Observable<void> {
    return this.gateway.delete(`${API_PATHS.INVENTORY_CATEGORIES}/${id}`);
  }

  addPresentation(productId: string, cmd: {
    displayName: string;
    presentationType: number;
    sellMode: number;
    brand?: string | null;
    nominalCapacity?: number | null;
    measureUnit: number;
    salePrice: number;
    costPrice: number;
  }): Observable<{ id: string }> {
    return this.gateway.post(API_PATHS.INVENTORY_PRESENTATIONS(productId), cmd);
  }

  getPresentations(productId: string): Observable<ProductPresentationDto[]> {
    return this.gateway.get<ProductPresentationDto[]>(API_PATHS.INVENTORY_PRESENTATIONS(productId));
  }

  deactivatePresentation(presentationId: string): Observable<void> {
    return this.gateway.delete(`/api/v1/inventory/presentations/${presentationId}`);
  }

  confirmStockEntry(cmd: ConfirmStockEntryRequest): Observable<{ stockEntryId: string }> {
    return this.gateway.post(API_PATHS.INVENTORY_STOCK_ENTRIES, cmd);
  }

  getStockEntries(page = 1, pageSize = 20): Observable<{ items: StockEntryDto[]; totalCount: number; totalPages: number }> {
    return this.gateway.get(API_PATHS.INVENTORY_STOCK_ENTRIES, { page, pageSize });
  }

  openContainer(containerId: string, cmd: OpenContainerRequest): Observable<void> {
    return this.gateway.post(`/api/v1/inventory/containers/${containerId}/open`, cmd);
  }

  drawFromContainer(containerId: string, cmd: DrawFromContainerRequest): Observable<{ remaining: number }> {
    return this.gateway.post(API_PATHS.INVENTORY_CONTAINER_DRAW(containerId), cmd);
  }

  markContainerEmpty(containerId: string): Observable<void> {
    return this.gateway.post(API_PATHS.INVENTORY_CONTAINER_EMPTY(containerId), {});
  }

  setActiveContainer(presentationId: string, cmd: SetActiveContainerRequest): Observable<void> {
    return this.gateway.put(API_PATHS.INVENTORY_ACTIVE_CONTAINER(presentationId), cmd);
  }

  getOpenContainers(presentationId: string): Observable<OpenContainerSummaryDto[]> {
    return this.gateway.get<OpenContainerSummaryDto[]>(API_PATHS.INVENTORY_OPEN_CONTAINERS(presentationId));
  }

  getContainersByPresentation(presentationId: string, status?: string): Observable<StockContainerDto[]> {
    const params: any = {};
    if (status) params['status'] = status;
    return this.gateway.get<StockContainerDto[]>(`/api/v1/inventory/presentations/${presentationId}/containers`, params);
  }
}
