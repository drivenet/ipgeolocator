using System;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class I2LDatabase
    {
        public I2LDatabase(
            I2LInterval4[] intervals,
            I2LLocation[] locations,
            string[] atoms,
            DateTime timestamp)
        {
            Intervals = intervals ?? throw new ArgumentNullException(nameof(intervals));
            Locations = locations ?? throw new ArgumentNullException(nameof(locations));
            Atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
            Timestamp = timestamp.ToUniversalTime();
        }

#pragma warning disable CA1819 // Properties should not return arrays -- required for performance reasons
        public I2LInterval4[] Intervals { get; }

        public I2LLocation[] Locations { get; }

        public string[] Atoms { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        public DateTime Timestamp { get; }
    }
}
