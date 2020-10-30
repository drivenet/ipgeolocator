using System;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Options;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class FileI2LDatabaseStreamFactory : II2LDatabaseStreamFactory
    {
        private readonly IOptionsMonitor<DatabaseOptions> _options;

        public FileI2LDatabaseStreamFactory(IOptionsMonitor<DatabaseOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Stream Open()
        {
            var fileName = _options.CurrentValue.DatabaseFileName ?? "IP-COUNTRY-REGION-CITY.DAT";
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Invalid I2L database file name.", nameof(fileName));
            }

            if (!Path.IsPathRooted(fileName))
            {
                if (Assembly.GetEntryAssembly()?.Location is { } path
                    && Path.GetDirectoryName(path) is { } root)
                {
                    fileName = Path.Combine(root, fileName);
                }
            }

            return File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
