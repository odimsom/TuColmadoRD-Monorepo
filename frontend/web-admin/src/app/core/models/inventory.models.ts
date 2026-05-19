export interface ProductDto {
  productId: string;
  name: string;
  categoryId: string;
  categoryName: string;
  itbisRate: number;
  isActive: boolean;
  updatedAt: string;
  presentations: ProductPresentationDto[];
}

export interface ProductPresentationDto {
  id: string;
  productId: string;
  displayName: string;
  presentationType: number;
  presentationTypeName: string;
  sellMode: number;
  sellModeName: string;
  brand: string | null;
  nominalCapacity: number | null;
  measureUnit: number;
  measureUnitName: string;
  salePrice: number;
  costPrice: number;
  isActive: boolean;
  stockQuantity: number;
  openContainersCount: number;
  packagedStockQuantity: number;
}

export interface StockContainerDto {
  id: string;
  presentationId: string;
  containerCode: string;
  nominalCapacity: number;
  actualCapacity: number | null;
  currentRemaining: number;
  status: number;
  statusName: string;
  isActiveSource: boolean;
  notes: string | null;
  purchasedAt: string;
  openedAt: string | null;
  emptiedAt: string | null;
}

export interface PackagedStockDto {
  id: string;
  presentationId: string;
  quantity: number;
  lastUpdatedAt: string;
}

export interface StockEntryDto {
  id: string;
  purchasedAt: string;
  totalCost: number;
  supplierName: string | null;
  notes: string | null;
  fundTransactionId: string | null;
  lines: StockEntryLineDto[];
}

export interface StockEntryLineDto {
  id: string;
  stockEntryId: string;
  presentationId: string;
  presentationDisplayName: string;
  containerCount: number;
  unitsPerContainer: number;
  nominalSizePerUnit: number;
  costPerUnit: number;
  lineTotal: number;
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
  itbisRate: number;
}

export interface AddProductPresentationRequest {
  displayName: string;
  presentationType: number;
  sellMode: number;
  brand?: string | null;
  nominalCapacity?: number | null;
  measureUnit: number;
  salePrice: number;
  costPrice: number;
}

export interface UpdatePresentationPriceRequest {
  salePrice: number;
  costPrice: number;
}

export interface StockEntryLineRequest {
  presentationId: string;
  containerCount: number;
  unitsPerContainer: number;
  nominalSizePerUnit: number;
  costPerUnit: number;
}

export interface ConfirmStockEntryRequest {
  purchasedAt?: string;
  supplierName?: string | null;
  notes?: string | null;
  fundId?: string | null;
  fundExpenseJustification?: string | null;
  lines: StockEntryLineRequest[];
}

export interface OpenContainerRequest {
  actualCapacity?: number | null;
}

export interface DrawFromContainerRequest {
  amount: number;
  allowOverDraw?: boolean;
}

export interface SetActiveContainerRequest {
  containerId: string;
}

export interface CategoryDto {
  id: string;
  name: string;
}

export interface LowStockPresentationDto {
  presentationId: string;
  productName: string;
  presentationDisplayName: string;
  stockQuantity: number;
  threshold: number;
}

export interface OpenContainerSummaryDto {
  id: string;
  containerCode: string;
  currentRemaining: number;
  nominalCapacity: number;
  statusName: string;
  isActiveSource: boolean;
  openedAt: string | null;
}
