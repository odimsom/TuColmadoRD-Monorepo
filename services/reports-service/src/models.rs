use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Deserialize)]
pub struct TenantQuery {
    pub tenant_id: Uuid,
    pub from: Option<String>, // YYYY-MM-DD
    pub to: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SalesReport {
    pub tenant_id: Uuid,
    pub period_from: String,
    pub period_to: String,
    pub total_revenue: f64,
    pub transaction_count: i64,
    pub average_ticket: f64,
    pub top_products: Vec<TopProduct>,
    pub generated_at: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, sqlx::FromRow)]
pub struct TopProduct {
    pub product_name: String,
    pub units_sold: f64,
    pub revenue: f64,
}

#[derive(Debug, Clone, Serialize, Deserialize, sqlx::FromRow)]
pub struct LowStockAlert {
    pub product_id: Uuid,
    pub product_name: String,
    pub category_name: Option<String>,
    pub stock_quantity: f64,
    pub sale_price: f64,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct InventoryAlertsReport {
    pub tenant_id: Uuid,
    pub low_stock: Vec<LowStockAlert>,
    pub generated_at: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CustomerReport {
    pub tenant_id: Uuid,
    pub total_customers: i64,
    pub with_debt: i64,
    pub total_debt: f64,
    pub generated_at: String,
}
