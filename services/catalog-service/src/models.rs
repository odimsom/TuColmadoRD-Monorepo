use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Clone, Serialize, Deserialize, sqlx::FromRow)]
pub struct Product {
    pub id:             Uuid,
    pub tenant_id:      Uuid,
    pub name:           String,
    pub category_id:    Uuid,
    pub category_name:  Option<String>,
    pub sale_price:     f64,
    pub stock_quantity: f64,
    pub is_active:      bool,
}

#[derive(Debug, Clone, Serialize, Deserialize, sqlx::FromRow)]
pub struct Category {
    pub id:        Uuid,
    pub tenant_id: Uuid,
    pub name:      String,
    pub is_active: bool,
}

#[derive(Debug, Deserialize)]
pub struct TenantQuery {
    pub tenant_id: Uuid,
}

#[derive(Debug, Serialize)]
pub struct HealthStatus {
    pub status:     &'static str,
    pub redis:      &'static str,
    pub database:   &'static str,
    pub db_cb:      &'static str,
    pub redis_cb:   &'static str,
}
