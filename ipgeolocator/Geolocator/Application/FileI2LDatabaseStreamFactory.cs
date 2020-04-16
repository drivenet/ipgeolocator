using System;
using System.IO;

namespace IpGeolocator.Geolocator.Application
{
    public sealed class FileI2LDatabaseStreamFactory : II2LDatabaseStreamFactory
    {
        private readonly string _fileName;

        public FileI2LDatabaseStreamFactory(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Invalid I2L database file name.", nameof(fileName));
            }

            _fileName = Path.GetFullPath(fileName);
        }

        public Stream Open()
            => File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
