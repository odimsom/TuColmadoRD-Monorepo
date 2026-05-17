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
    pub redis_cb: resilience::CircuitBreaker,
    pub metrics: Arc<metrics::Metrics>,
}

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    dotenvy::dotenv().ok();

    tracing_subscriber::fmt()
        .with_env_filter(EnvFilter::from_default_env())
        .json()
        .init();

    let cfg = Config::from_env()?;

    let db = db::connect(&cfg.database_url).await?;
    let redis = cache::connect(&cfg.redis_url).await?;

    let state = Arc::new(AppState {
        db,
        redis,
        db_cb: resilience::CircuitBreaker::new(5, std::time::Duration::from_secs(30)),
        redis_cb: resilience::CircuitBreaker::new(3, std::time::Duration::from_secs(15)),
        metrics: Arc::new(metrics::Metrics::new()),
    });

    let app = Router::new()
        .route("/health", get(handlers::health))
        .route("/catalog", get(handlers::get_catalog))
        .route("/catalog/categories", get(handlers::get_categories))
        .route("/metrics", get(handlers::prometheus_metrics))
        .layer(TraceLayer::new_for_http())
        .layer(CorsLayer::permissive())
        .with_state(state);

    let addr = format!("0.0.0.0:{}", cfg.port);
    tracing::info!(port = cfg.port, "catalog-service starting");
    let listener = tokio::net::TcpListener::bind(&addr).await?;
    axum::serve(listener, app).await?;
    Ok(())
}
