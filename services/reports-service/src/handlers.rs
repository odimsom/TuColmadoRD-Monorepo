use std::sync::Arc;
use axum::{
    extract::{Query, State},
    http::StatusCode,
    response::{IntoResponse, Response},
    Json,
};
use chrono::{Datelike, Local};

use crate::{
    AppState, cache, db, resilience,
    models::{CustomerReport, InventoryAlertsReport, SalesReport, TenantQuery},
};

fn today() -> String { Local::now().format("%Y-%m-%d").to_string() }
fn month_start() -> String {
    let n = Local::now();
    format!("{}-{:02}-01", n.year(), n.month())
}

// ── GET /reports/sales ────────────────────────────────────────────────────────
//
// Lazy computation + Redis cache:
//   - Cache key includes tenant + date range (invalidates automatically per day)
//   - Compute only on cache miss — never eagerly
//
pub async fn sales_report(
    State(state): State<Arc<AppState>>,
    Query(q): Query<TenantQuery>,
) -> Response {
    let from = q.from.clone().unwrap_or_else(month_start);
    let to   = q.to.clone().unwrap_or_else(today);
    let cache_key = format!("report:sales:{}:{}:{}", q.tenant_id, from, to);

    // Lazy: try cache first
    let mut conn = state.redis.clone();
    if let Some(cached) = cache::get::<SalesReport>(&mut conn, &cache_key).await {
        return Json(cached).into_response();
    }

    // Compute on demand
    let pool  = state.db.clone();
    let tid   = q.tenant_id;
    let from2 = from.clone();
    let to2   = to.clone();

    let summary = match resilience::call(&state.db_cb, "pg:sales-summary", || {
        db::fetch_sales_summary(&pool, tid, &from2, &to2)
    })
    .await
    {
        Ok(s) => s,
        Err(e) => {
            state.metrics.http_requests.with_label_values(&["GET", "/reports/sales", "503"]).inc();
            return (StatusCode::SERVICE_UNAVAILABLE, Json(serde_json::json!({
                "error": "report unavailable", "detail": e.to_string()
            }))).into_response();
        }
    };

    let pool2 = state.db.clone();
    let top = resilience::call(&state.db_cb, "pg:top-products", || {
        db::fetch_top_products(&pool2, tid, &from, &to, 5)
    })
    .await
    .unwrap_or_default();

    let avg = if summary.transaction_count > 0 {
        summary.total_revenue / summary.transaction_count as f64
    } else {
        0.0
    };

    let report = SalesReport {
        tenant_id:         q.tenant_id,
        period_from:       from,
        period_to:         to,
        total_revenue:     summary.total_revenue,
        transaction_count: summary.transaction_count,
        average_ticket:    avg,
        top_products:      top,
        generated_at:      chrono::Utc::now().to_rfc3339(),
    };

    // Lazy populate cache (10-minute TTL for same date range)
    let mut conn2 = state.redis.clone();
    let r2 = report.clone();
    let key2 = cache_key.clone();
    tokio::spawn(async move { cache::set(&mut conn2, &key2, &r2, 600).await });

    state.metrics.http_requests.with_label_values(&["GET", "/reports/sales", "200"]).inc();
    Json(report).into_response()
}

// ── GET /reports/inventory-alerts ─────────────────────────────────────────────

pub async fn inventory_alerts(
    State(state): State<Arc<AppState>>,
    Query(q): Query<TenantQuery>,
) -> Response {
    let cache_key = format!("report:inventory-alerts:{}", q.tenant_id);
    let mut conn = state.redis.clone();

    if let Some(cached) = cache::get::<InventoryAlertsReport>(&mut conn, &cache_key).await {
        return Json(cached).into_response();
    }

    let pool = state.db.clone();
    match resilience::call(&state.db_cb, "pg:low-stock", || {
        db::fetch_low_stock(&pool, q.tenant_id, 5.0) // threshold = 5 units
    })
    .await
    {
        Ok(alerts) => {
            let report = InventoryAlertsReport {
                tenant_id:    q.tenant_id,
                low_stock:    alerts,
                generated_at: chrono::Utc::now().to_rfc3339(),
            };
            let mut conn2 = state.redis.clone();
            let r2 = report.clone();
            let key2 = cache_key.clone();
            tokio::spawn(async move { cache::set(&mut conn2, &key2, &r2, 120).await }); // 2min TTL
            Json(report).into_response()
        }
        Err(e) => (StatusCode::SERVICE_UNAVAILABLE, Json(serde_json::json!({
            "error": "inventory report unavailable"
        }))).into_response(),
    }
}

// ── GET /reports/customers ─────────────────────────────────────────────────────

pub async fn customer_report(
    State(state): State<Arc<AppState>>,
    Query(q): Query<TenantQuery>,
) -> Response {
    let cache_key = format!("report:customers:{}", q.tenant_id);
    let mut conn = state.redis.clone();

    if let Some(cached) = cache::get::<CustomerReport>(&mut conn, &cache_key).await {
        return Json(cached).into_response();
    }

    let pool = state.db.clone();
    match resilience::call(&state.db_cb, "pg:customer-stats", || {
        db::fetch_customer_stats(&pool, q.tenant_id)
    })
    .await
    {
        Ok((total, with_debt, total_debt)) => {
            let report = CustomerReport {
                tenant_id: q.tenant_id,
                total_customers: total,
                with_debt,
                total_debt,
                generated_at: chrono::Utc::now().to_rfc3339(),
            };
            let mut conn2 = state.redis.clone();
            let r2 = report.clone();
            let key2 = cache_key.clone();
            tokio::spawn(async move { cache::set(&mut conn2, &key2, &r2, 300).await });
            Json(report).into_response()
        }
        Err(_) => (StatusCode::SERVICE_UNAVAILABLE, Json(serde_json::json!({
            "error": "customer report unavailable"
        }))).into_response(),
    }
}

// ── GET /health ────────────────────────────────────────────────────────────────

pub async fn health(State(state): State<Arc<AppState>>) -> Response {
    let db_ok = db::ping(&state.db).await.is_ok();
    let status = if db_ok { StatusCode::OK } else { StatusCode::SERVICE_UNAVAILABLE };
    (status, Json(serde_json::json!({
        "status":   if db_ok { "ok" } else { "degraded" },
        "database": if db_ok { "up" } else { "down" },
        "db_cb":    state.db_cb.state_name(),
    }))).into_response()
}

// ── GET /metrics ───────────────────────────────────────────────────────────────

pub async fn prometheus_metrics(State(state): State<Arc<AppState>>) -> impl IntoResponse {
    state.metrics.cb_state
        .with_label_values(&["db"])
        .set(if state.db_cb.state_name() == "open" { 1 } else { 0 });
    (
        StatusCode::OK,
        [("content-type", "text/plain; version=0.0.4")],
        state.metrics.gather(),
    )
}
