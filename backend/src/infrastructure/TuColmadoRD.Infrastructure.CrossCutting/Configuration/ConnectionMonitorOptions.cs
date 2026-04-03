namespace TuColmadoRD.Infrastructure.CrossCutting.Configuration
{
    public sealed class ConnectionMonitorOptions
    {
        public const string SectionName = "ConnectionMonitor";
        public TimeSpan CheckInterval { get; init; } = TimeSpan.FromSeconds(15);
        public TimeSpan PingTimeout { get; init; } = TimeSpan.FromSeconds(2);
        public TimeSpan EventDebounce { get; init; } = TimeSpan.FromSeconds(2);
        public TimeSpan CircuitBreakerCooldown { get; init; } = TimeSpan.FromMinutes(1);
        public int CircuitBreakerThreshold { get; init; } = 5;
        public string[] PingEndpoints { get; init; } = ["8.8.8.8", "1.1.1.1", "9.9.9.9"];
    }
}
