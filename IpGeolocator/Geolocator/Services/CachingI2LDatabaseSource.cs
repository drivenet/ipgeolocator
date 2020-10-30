using System;
using System.Threading;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class CachingI2LDatabaseSource : II2LDatabaseSource, IDisposable
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(7);

        private readonly II2LDatabaseReader _inner;
        private readonly Timer _timer;
        private volatile I2LDatabase? _database;

        public CachingI2LDatabaseSource(II2LDatabaseReader inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _timer = new Timer(Refresh, null, TimeSpan.Zero, RefreshInterval);
        }

        public I2LDatabase Database => _database ?? ReadDatabase();

        public void Dispose() => _timer.Dispose();

        private I2LDatabase ReadDatabase()
        {
#pragma warning disable CA2002 // Do not lock on objects with weak identity -- locally-constructed object os ok
            lock (_timer)
#pragma warning restore CA2002 // Do not lock on objects with weak identity
            {
                return _database ??= _inner.ReadDatabase();
            }
        }

        private void Refresh(object? state)
        {
            var lockTaken = false;
            try
            {
#pragma warning disable CA2002 // Do not lock on objects with weak identity -- unpublished readonly reference
                Monitor.TryEnter(_timer, ref lockTaken);
#pragma warning restore CA2002 // Do not lock on objects with weak identity
                if (lockTaken)
                {
                    Refresh();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_timer);
                }
            }
        }

        private void Refresh()
        {
            try
            {
                _database = _inner.ReadDatabase();
            }
#pragma warning disable CA1031 // Do not catch general exception types -- robustness is critical in this case
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }
        }
    }
}
