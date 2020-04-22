using System;

using Microsoft.Extensions.Logging;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed partial class LoggingI2LDatabaseSource : II2LDatabaseSource
    {
        private readonly II2LDatabaseSource _inner;
        private readonly ILogger _logger;

        public LoggingI2LDatabaseSource(II2LDatabaseSource inner, ILogger<LoggingI2LDatabaseSource> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public I2LDatabase Database
        {
            get
            {
                I2LDatabase result;
                _logger.LogInformation(EventIds.Loading, "Loading I2L database.");
                try
                {
                    result = _inner.Database;
                }
                catch (Exception exception)
                {
                    _logger.LogError(EventIds.LoadFailed, exception, "Failed to load I2L database.");
                    throw;
                }

                _logger.LogInformation(EventIds.Loaded, "Loaded I2L database.");
                return result;
            }
        }
    }
}
