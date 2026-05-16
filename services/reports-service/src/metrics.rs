use prometheus::{
    Encoder, HistogramOpts, HistogramVec, IntCounterVec, IntGaugeVec, Opts, Registry,
    TextEncoder,
};

pub struct Metrics {
    pub registry:      Registry,
    pub http_requests: IntCounterVec,
    pub http_latency:  HistogramVec,
    pub cache_ops:     IntCounterVec,  // labels: op=hit|miss|error, endpoint
    pub cb_state:      IntGaugeVec,    // labels: breaker=db, state=0(closed)/1(open)
}

impl Metrics {
    pub fn new() -> Self {
        let registry = Registry::new();

        let http_requests = IntCounterVec::new(
            Opts::new("reports_http_requests_total", "Total HTTP requests"),
            &["method", "endpoint", "status"],
        ).unwrap();

        let http_latency = HistogramVec::new(
            HistogramOpts::new("reports_http_duration_seconds", "HTTP request duration")
                .buckets(vec![0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1.0, 2.5, 5.0, 10.0]),
            &["endpoint"],
        ).unwrap();

        let cache_ops = IntCounterVec::new(
            Opts::new("reports_cache_ops_total", "Cache hit/miss/error"),
            &["op", "endpoint"],
        ).unwrap();

        let cb_state = IntGaugeVec::new(
            Opts::new("reports_circuit_breaker_open", "1 if circuit breaker is open"),
            &["breaker"],
        ).unwrap();

        registry.register(Box::new(http_requests.clone())).unwrap();
        registry.register(Box::new(http_latency.clone())).unwrap();
        registry.register(Box::new(cache_ops.clone())).unwrap();
        registry.register(Box::new(cb_state.clone())).unwrap();

        Self { registry, http_requests, http_latency, cache_ops, cb_state }
    }

    pub fn gather(&self) -> String {
        let mut buf = Vec::new();
        TextEncoder::new()
            .encode(&self.registry.gather(), &mut buf)
            .unwrap_or_default();
        String::from_utf8(buf).unwrap_or_default()
    }
}
