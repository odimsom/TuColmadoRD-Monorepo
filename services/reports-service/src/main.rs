mod cache;
mod config;
mod db;
mod handlers;
mod metrics;
mod models;
mod resilience;

use axum::{routing::get, Router};
use std::sync::Arc;
use tower_http::cors::CorsLayer;
use tower_http::trace::TraceLayer;
use tracing_subscriber::EnvFilter;

pub use config::Config;

#[derive(Clone)]
pub struct AppState {
    pub db: db::DbPool,
    pub redis: cache::RedisPool,
    pub db_cb: resilience::CircuitBreaker,
    pub metrics: Arc<metrics::Metrics>,
    pub cfg: Arc<Config>,
}

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    dotenvy::dotenv().ok();

    tracing_subscriber::fmt()
        .with_env_filter(EnvFilter::from_default_env())
        .json()
        .init();

    let cfg = Arc::new(Config::from_env()?);
    let db = db::connect(&cfg.database_url).await?;
    let redis = cache::connect(&cfg.redis_url).await?;

    let state = Arc::new(AppState {
        db,
        redis,
        db_cb: resilience::CircuitBreaker::new(5, std::time::Duration::from_secs(30)),
        metrics: Arc::new(metrics::Metrics::new()),
        cfg: cfg.clone(),
    });

    let app = Router::new()
        .route("/health", get(handlers::health))
        .route("/reports/sales", get(handlers::sales_report))
        .route("/reports/inventory-alerts", get(handlers::inventory_alerts))
        .route("/reports/customers", get(handlers::customer_report))
        .route("/metrics", get(handlers::prometheus_metrics))
        .layer(TraceLayer::new_for_http())
        .layer(CorsLayer::permissive())
        .with_state(state);

    let addr = format!("0.0.0.0:{}", cfg.port);
    tracing::info!(port = cfg.port, "reports-service starting");
    let listener = tokio::net::TcpListener::bind(&addr).await?;
    axum::serve(listener, app).await?;
    Ok(())
}
