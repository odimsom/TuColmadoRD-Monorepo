use std::sync::Arc;
use axum::{
    extract::{Query, State},
    http::StatusCode,
    response::{IntoResponse, Response},
    Json,
};

use crate::{
    AppState,
    cache,
    db,
    metrics::Timer,
    models::{Category, HealthStatus, Product, TenantQuery},
    resilience,
};

// ── GET /catalog ──────────────────────────────────────────────────────────────
//
// Lazy-load pattern:
//   1. Check Redis (fast path, circuit-broken)
//   2. On cache MISS → load from PostgreSQL (circuit-broken)
//   3. Populate Redis for next request (best-effort)
//   4. If both fail → 503 with structured error
//
pub async fn get_catalog(
    State(state): State<Arc<AppState>>,
    Query(q): Query<TenantQuery>,
) -> Response {
    let _timer = Timer::new(&state.metrics, "/catalog");
    let cache_key = format!("catalog:{}", q.tenant_id);

    // Fast path — try Redis
    if state.redis_cb.is_available() {
        let conn = state.redis.clone();
        let ck = cache_key.clone();
        match resilience::call(&state.redis_cb, "redis:get-catalog", move || {
            let mut conn = conn.clone();
            let ck = ck.clone();
            async move { Ok::<_, anyhow::Error>(cache::get::<Vec<Product>>(&mut conn, &ck).await) }
        })
        .await
        {
            Ok(Some(products)) => {
                state.metrics.cache_ops.with_label_values(&["hit", "/catalog"]).inc();
                state.metrics.http_requests.with_label_values(&["GET", "/catalog", "200"]).inc();
                return Json(products).into_response();
            }
            Ok(None) => {
                state.metrics.cache_ops.with_label_values(&["miss", "/catalog"]).inc();
            }
            Err(_) => {
                state.metrics.cache_ops.with_label_values(&["error", "/catalog"]).inc();
            }
        }
    }

    // Slow path — load from PostgreSQL
    let pool = state.db.clone();
    match resilience::call(&state.db_cb, "pg:fetch-catalog", || {
        let pool = pool.clone();
        let tid  = q.tenant_id;
        async move { db::fetch_catalog(&pool, tid).await }
    })
    .await
    {
        Ok(products) => {
            // Populate cache lazily (best-effort, don't block response)
            if state.redis_cb.is_available() {
                let mut conn = state.redis.clone();
                let key = cache_key.clone();
                let data = products.clone();
                let ttl = 300u64;
                tokio::spawn(async move {
                    cache::set(&mut conn, &key, &data, ttl).await;
                });
            }
            state.metrics.http_requests.with_label_values(&["GET", "/catalog", "200"]).inc();
            Json(products).into_response()
        }
        Err(e) => {
            tracing::error!(tenant_id = %q.tenant_id, error = %e, "catalog fetch failed");
            state.metrics.http_requests.with_label_values(&["GET", "/catalog", "503"]).inc();
            (StatusCode::SERVICE_UNAVAILABLE, Json(serde_json::json!({
                "error": "catalog temporarily unavailable",
                "detail": e.to_string()
            }))).into_response()
        }
    }
}

// ── GET /catalog/categories ───────────────────────────────────────────────────

pub async fn get_categories(
    State(state): State<Arc<AppState>>,
    Query(q): Query<TenantQuery>,
) -> Response {
    let _timer = Timer::new(&state.metrics, "/catalog/categories");
    let cache_key = format!("categories:{}", q.tenant_id);

    if state.redis_cb.is_available() {
        let conn = state.redis.clone();
        let ck = cache_key.clone();
        if let Ok(Some(cats)) = resilience::call(&state.redis_cb, "redis:get-cats", move || {
            let mut conn = conn.clone();
            let ck = ck.clone();
            async move { Ok::<_, anyhow::Error>(cache::get::<Vec<Category>>(&mut conn, &ck).await) }
        })
        .await
        {
            state.metrics.cache_ops.with_label_values(&["hit", "/catalog/categories"]).inc();
            return Json(cats).into_response();
        }
        state.metrics.cache_ops.with_label_values(&["miss", "/catalog/categories"]).inc();
    }

    let pool = state.db.clone();
    match resilience::call(&state.db_cb, "pg:fetch-cats", || {
        let pool = pool.clone();
        let tid  = q.tenant_id;
        async move { db::fetch_categories(&pool, tid).await }
    })
    .await
    {
        Ok(cats) => {
            if state.redis_cb.is_available() {
                let mut conn = state.redis.clone();
                let key = cache_key.clone();
                let data = cats.clone();
                tokio::spawn(async move { cache::set(&mut conn, &key, &data, 600).await });
            }
            Json(cats).into_response()
        }
        Err(_) => {
            (StatusCode::SERVICE_UNAVAILABLE, Json(serde_json::json!({
                "error": "categories temporarily unavailable"
            }))).into_response()
        }
    }
}

// ── GET /health ───────────────────────────────────────────────────────────────

pub async fn health(State(state): State<Arc<AppState>>) -> Response {
    let mut redis_ok = false;
    let mut db_ok = false;

    if state.redis_cb.is_available() {
        let mut conn = state.redis.clone();
        redis_ok = cache::ping(&mut conn).await.is_ok();
    }
    if state.db_cb.is_available() {
        db_ok = db::ping(&state.db).await.is_ok();
    }

    let all_ok = redis_ok && db_ok;
    let status = if all_ok { StatusCode::OK } else { StatusCode::SERVICE_UNAVAILABLE };

    (status, Json(HealthStatus {
        status:   if all_ok { "ok" } else { "degraded" },
        redis:    if redis_ok { "up" } else { "down" },
        database: if db_ok   { "up" } else { "down" },
        db_cb:    state.db_cb.state_name(),
        redis_cb: state.redis_cb.state_name(),
    })).into_response()
}

// ── GET /metrics ──────────────────────────────────────────────────────────────

pub async fn prometheus_metrics(State(state): State<Arc<AppState>>) -> impl IntoResponse {
    // Update circuit breaker gauges before scrape
    state.metrics.cb_state
        .with_label_values(&["redis"])
        .set(if state.redis_cb.state_name() == "open" { 1 } else { 0 });
    state.metrics.cb_state
        .with_label_values(&["db"])
        .set(if state.db_cb.state_name() == "open" { 1 } else { 0 });

    (
        StatusCode::OK,
        [("content-type", "text/plain; version=0.0.4")],
        state.metrics.gather(),
    )
}
