using System;

namespace IpGeolocator.Application
{
    internal sealed class I2LDatabase
    {
        public I2LDatabase(
            I2LInterval4[] intervals,
            I2LLocation[] locations,
            string[] atoms)
        {
            Intervals = intervals ?? throw new ArgumentNullException(nameof(intervals));
            Locations = locations ?? throw new ArgumentNullException(nameof(locations));
            Atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        public I2LInterval4[] Intervals { get; }

        public I2LLocation[] Locations { get; }

        public string[] Atoms { get; }
    }
}
