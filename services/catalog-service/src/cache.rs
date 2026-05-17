use redis::{aio::ConnectionManager, AsyncCommands, Client};
use serde::{de::DeserializeOwned, Serialize};

pub type RedisPool = ConnectionManager;

pub async fn connect(url: &str) -> anyhow::Result<RedisPool> {
    let client = Client::open(url)?;
    let mgr = ConnectionManager::new(client).await?;
    tracing::info!("Redis connected");
    Ok(mgr)
}

/// Lazy-get: return cached value if present, else return None (caller loads from DB).
pub async fn get<T: DeserializeOwned>(conn: &mut RedisPool, key: &str) -> Option<T> {
    let raw: Option<String> = conn.get(key).await.ok()?;
    raw.and_then(|s| serde_json::from_str(&s).ok())
}

/// Store value with TTL (seconds). Silently ignores errors — cache is best-effort.
pub async fn set<T: Serialize>(conn: &mut RedisPool, key: &str, value: &T, ttl_s: u64) {
    if let Ok(json) = serde_json::to_string(value) {
        let _: redis::RedisResult<()> = conn.set_ex(key, json, ttl_s).await;
    }
}

pub async fn ping(conn: &mut RedisPool) -> anyhow::Result<()> {
    let _: String = redis::cmd("PING").query_async(conn).await?;
    Ok(())
}
