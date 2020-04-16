using System;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace IpGeolocator.Geolocator.Application
{
    internal sealed class CachingI2LDatabaseSource : II2LDatabaseSource, IDisposable
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(3);

        private readonly II2LDatabaseSource _inner;
        private readonly ILogger? _logger;
        private readonly Timer _timer;
        private volatile I2LDatabase? _database;

        public CachingI2LDatabaseSource(II2LDatabaseSource inner, ILogger<CachingI2LDatabaseSource>? logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger;
            _timer = new Timer(Refresh, null, TimeSpan.Zero, RefreshInterval);
        }

        public I2LDatabase Database => _database ?? GetDatabase();

        public void Dispose() => _timer.Dispose();

        private I2LDatabase GetDatabase()
        {
#pragma warning disable CA2002 // Do not lock on objects with weak identity -- locally-constructed object os ok
            lock (_timer)
#pragma warning restore CA2002 // Do not lock on objects with weak identity
            {
                var database = _database;
                if (database is null)
                {
                    _database = database = _inner.Database;
                }

                return database;
            }
        }

        private void Refresh(object? state)
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_timer, ref lockTaken);
                if (lockTaken)
                {
                    _database = _inner.Database;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types -- robustness is critical in this case
            catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(exception, "Failed to refresh cached I2L database.");
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_timer);
                }
            }
        }
    }
}
