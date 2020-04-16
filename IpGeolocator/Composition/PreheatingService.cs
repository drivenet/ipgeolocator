using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using IpGeolocator.Geolocator;
using IpGeolocator.Metrics;
using Microsoft.Extensions.Hosting;

namespace IpGeolocator.Composition
{
    internal sealed class PreheatingService : IHostedService
    {
        private readonly IGeolocator _geolocator;
        private readonly IMetricsManager _metricsManager;

        public PreheatingService(IGeolocator geolocator, IMetricsManager metricsManager)
        {
            _geolocator = geolocator ?? throw new ArgumentNullException(nameof(geolocator));
            _metricsManager = metricsManager ?? throw new ArgumentNullException(nameof(metricsManager));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _geolocator.Geolocate(IPAddress.Loopback);
                _metricsManager.Reset();
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
