use prometheus::{
    Encoder, HistogramOpts, HistogramVec, IntCounterVec, IntGaugeVec, Opts, Registry, TextEncoder,
};
use std::time::Instant;

pub struct Metrics {
    pub registry: Registry,
    pub http_requests: IntCounterVec,
    pub http_latency: HistogramVec,
    pub cache_ops: IntCounterVec, // labels: op=hit|miss|error, endpoint
    pub cb_state: IntGaugeVec,    // labels: breaker=redis|db, state=0(closed)/1(open)
}

impl Metrics {
    pub fn new() -> Self {
        let registry = Registry::new();

        let http_requests = IntCounterVec::new(
            Opts::new("catalog_http_requests_total", "Total HTTP requests"),
            &["method", "endpoint", "status"],
        )
        .unwrap();

        let http_latency = HistogramVec::new(
            HistogramOpts::new("catalog_http_duration_seconds", "HTTP request duration")
                .buckets(vec![0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1.0, 2.5]),
            &["endpoint"],
        )
        .unwrap();

        let cache_ops = IntCounterVec::new(
            Opts::new("catalog_cache_ops_total", "Cache hit/miss/error"),
            &["op", "endpoint"],
        )
        .unwrap();

        let cb_state = IntGaugeVec::new(
            Opts::new(
                "catalog_circuit_breaker_open",
                "1 if circuit breaker is open",
            ),
            &["breaker"],
        )
        .unwrap();

        registry.register(Box::new(http_requests.clone())).unwrap();
        registry.register(Box::new(http_latency.clone())).unwrap();
        registry.register(Box::new(cache_ops.clone())).unwrap();
        registry.register(Box::new(cb_state.clone())).unwrap();

        Self {
            registry,
            http_requests,
            http_latency,
            cache_ops,
            cb_state,
        }
    }

    pub fn gather(&self) -> String {
        let mut buf = Vec::new();
        TextEncoder::new()
            .encode(&self.registry.gather(), &mut buf)
            .unwrap_or_default();
        String::from_utf8(buf).unwrap_or_default()
    }
}

// RAII timer that records latency on drop.
pub struct Timer<'a> {
    start: Instant,
    metrics: &'a Metrics,
    endpoint: &'a str,
}

impl<'a> Timer<'a> {
    pub fn new(metrics: &'a Metrics, endpoint: &'a str) -> Self {
        Self {
            start: Instant::now(),
            metrics,
            endpoint,
        }
    }
}

impl Drop for Timer<'_> {
    fn drop(&mut self) {
        self.metrics
            .http_latency
            .with_label_values(&[self.endpoint])
            .observe(self.start.elapsed().as_secs_f64());
    }
}
