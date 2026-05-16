use anyhow::{Context, Result};

#[derive(Debug, Clone)]
pub struct Config {
    pub port:         u16,
    pub database_url: String,
    pub redis_url:    String,
    pub cache_ttl_s:  u64,
}

impl Config {
    pub fn from_env() -> Result<Self> {
        Ok(Self {
            port: std::env::var("PORT")
                .unwrap_or_else(|_| "8081".into())
                .parse()
                .context("PORT must be a valid u16")?,
            database_url: std::env::var("DATABASE_URL")
                .context("DATABASE_URL is required")?,
            redis_url: std::env::var("REDIS_URL")
                .unwrap_or_else(|_| "redis://redis:6379".into()),
            cache_ttl_s: std::env::var("CACHE_TTL_S")
                .unwrap_or_else(|_| "600".into())
                .parse()
                .unwrap_or(600),
        })
    }
}
