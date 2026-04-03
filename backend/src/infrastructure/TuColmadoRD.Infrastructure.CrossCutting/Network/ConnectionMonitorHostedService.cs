using Microsoft.Extensions.Hosting;
using TuColmadoRD.Core.Application.Interfaces.Infrastructure.CrossCutting.Network;

namespace TuColmadoRD.Infrastructure.CrossCutting.Network
{
    internal sealed class ConnectionMonitorHostedService : IHostedService
    {
        private readonly IConnectionMonitor _monitor;

        public ConnectionMonitorHostedService(IConnectionMonitor monitor) =>
            _monitor = monitor;

        public Task StartAsync(CancellationToken ct) => _monitor.StartAsync(ct);
        public Task StopAsync(CancellationToken ct) => _monitor.DisposeAsync().AsTask();
    }
}
