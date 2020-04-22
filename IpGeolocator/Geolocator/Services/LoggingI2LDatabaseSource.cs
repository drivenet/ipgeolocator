using System;

using Microsoft.Extensions.Logging;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed partial class LoggingI2LDatabaseReader : II2LDatabaseReader
    {
        private readonly II2LDatabaseReader _inner;
        private readonly ILogger _logger;

        public LoggingI2LDatabaseReader(II2LDatabaseReader inner, ILogger<LoggingI2LDatabaseReader> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public I2LDatabase ReadDatabase()
        {
            I2LDatabase result;
            _logger.LogInformation(EventIds.Reading, "Reading I2L database.");
            try
            {
                result = _inner.ReadDatabase();
            }
            catch (Exception exception)
            {
                _logger.LogError(EventIds.ReadFailed, exception, "Failed to read I2L database.");
                throw;
            }

            _logger.LogInformation(EventIds.Read, "Reading I2L database.");
            return result;
        }
    }
}
