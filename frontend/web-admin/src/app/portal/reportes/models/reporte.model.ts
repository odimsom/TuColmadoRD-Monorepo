// Respuestas del reports-service (Rust) — llegan en snake_case
export interface TopProducto {
  product_name: string;
  units_sold: number;
  revenue: number;
}

export interface ReporteVentas {
  tenant_id: string;
  period_from: string;
  period_to: string;
  total_revenue: number;
  transaction_count: number;
  average_ticket: number;
  top_products: TopProducto[];
  generated_at: string;
}

export interface AlertaStock {
  product_id: string;
  product_name: string;
  category_name: string | null;
  stock_quantity: number;
  sale_price: number;
}

export interface ReporteInventario {
  tenant_id: string;
  low_stock: AlertaStock[];
  generated_at: string;
}

export interface ReporteClientes {
  tenant_id: string;
  total_customers: number;
  with_debt: number;
  total_debt: number;
  generated_at: string;
}
