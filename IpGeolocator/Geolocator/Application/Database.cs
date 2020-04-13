using System;

namespace IpGeolocator.Geolocator.Application
{
    internal sealed class Database
    {
        public Database(
            LocationV4Record[] records,
            Location[] locations,
            string[] atoms)
        {
            Records = records ?? throw new ArgumentNullException(nameof(records));
            Locations = locations ?? throw new ArgumentNullException(nameof(locations));
            Atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        public LocationV4Record[] Records { get; }

        public Location[] Locations { get; }

        public string[] Atoms { get; }
    }
}
