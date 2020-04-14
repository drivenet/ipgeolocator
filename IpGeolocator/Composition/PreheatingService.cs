using System;
using System.Threading;
using System.Threading.Tasks;

using IpGeolocator.Application;

using Microsoft.Extensions.Hosting;

namespace IpGeolocator.Composition
{
    internal sealed class PreheatingService : IHostedService
    {
        private readonly II2LDatabaseSource _databaseSource;

        public PreheatingService(II2LDatabaseSource databaseSource)
        {
            _databaseSource = databaseSource ?? throw new ArgumentNullException(nameof(databaseSource));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => _ = _databaseSource.Database);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
