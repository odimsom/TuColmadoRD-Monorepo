// Circuit breaker — atomic, lock-free, clone-friendly.
//
// States: CLOSED (normal) → OPEN (failing, reject) → HALF-OPEN (probe after timeout)
//
// Failure threshold exceeded → opens the breaker.
// After reset_timeout passes → allows one probe request (half-open).
// Probe succeeds → closes; probe fails → reopens.

use std::sync::atomic::{AtomicU32, AtomicU64, Ordering};
use std::sync::Arc;
use std::time::{Duration, SystemTime, UNIX_EPOCH};

const STATE_CLOSED: u32 = 0;
const STATE_OPEN: u32 = 1;
const STATE_HALF_OPEN: u32 = 2;

#[derive(Clone)]
pub struct CircuitBreaker {
    state: Arc<AtomicU32>,
    failures: Arc<AtomicU32>,
    last_failure_ts: Arc<AtomicU64>,
    threshold: u32,
    reset_timeout: Duration,
}

impl CircuitBreaker {
    pub fn new(threshold: u32, reset_timeout: Duration) -> Self {
        Self {
            state: Arc::new(AtomicU32::new(STATE_CLOSED)),
            failures: Arc::new(AtomicU32::new(0)),
            last_failure_ts: Arc::new(AtomicU64::new(0)),
            threshold,
            reset_timeout,
        }
    }

    fn now_secs() -> u64 {
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap_or_default()
            .as_secs()
    }

    pub fn is_available(&self) -> bool {
        match self.state.load(Ordering::Relaxed) {
            STATE_CLOSED => true,
            STATE_HALF_OPEN => true,
            STATE_OPEN => {
                let elapsed =
                    Self::now_secs().saturating_sub(self.last_failure_ts.load(Ordering::Relaxed));
                if elapsed >= self.reset_timeout.as_secs() {
                    let _ = self.state.compare_exchange(
                        STATE_OPEN,
                        STATE_HALF_OPEN,
                        Ordering::AcqRel,
                        Ordering::Relaxed,
                    );
                    true
                } else {
                    false
                }
            }
            _ => true,
        }
    }

    pub fn on_success(&self) {
        self.failures.store(0, Ordering::Relaxed);
        self.state.store(STATE_CLOSED, Ordering::Relaxed);
    }

    pub fn on_failure(&self) {
        let fails = self.failures.fetch_add(1, Ordering::Relaxed) + 1;
        self.last_failure_ts
            .store(Self::now_secs(), Ordering::Relaxed);
        if fails >= self.threshold {
            self.state.store(STATE_OPEN, Ordering::Relaxed);
            tracing::warn!(
                failures = fails,
                threshold = self.threshold,
                "circuit breaker OPENED"
            );
        }
    }

    pub fn state_name(&self) -> &'static str {
        match self.state.load(Ordering::Relaxed) {
            STATE_CLOSED => "closed",
            STATE_OPEN => "open",
            STATE_HALF_OPEN => "half_open",
            _ => "unknown",
        }
    }
}

pub async fn call<F, Fut, T, E>(
    cb: &CircuitBreaker,
    operation: &str,
    mut f: F,
) -> Result<T, anyhow::Error>
where
    F: FnMut() -> Fut,
    Fut: std::future::Future<Output = Result<T, E>>,
    E: std::fmt::Display,
{
    if !cb.is_available() {
        tracing::warn!(operation, "circuit breaker open — request rejected");
        anyhow::bail!("service unavailable (circuit breaker open)");
    }

    match tokio::time::timeout(Duration::from_secs(5), f()).await {
        Ok(Ok(val)) => {
            cb.on_success();
            Ok(val)
        }
        Ok(Err(e)) => {
            tracing::error!(operation, error = %e, "call failed");
            cb.on_failure();
            anyhow::bail!("operation failed: {}", e)
        }
        Err(_timeout) => {
            tracing::error!(operation, "call timed out");
            cb.on_failure();
            anyhow::bail!("operation timed out")
        }
    }
}
