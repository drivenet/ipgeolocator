using System;

namespace IpGeolocator.Geolocator.Application
{
    internal readonly struct Location : IEquatable<Location>
    {
        public Location(int countryIndex, int regionIndex, int cityIndex)
        {
            CountryIndex = countryIndex;
            RegionIndex = regionIndex;
            CityIndex = cityIndex;
        }

        public int CountryIndex { get; }

        public int RegionIndex { get; }

        public int CityIndex { get; }

        public static bool operator ==(Location left, Location right) => left.Equals(right);

        public static bool operator !=(Location left, Location right) => !(left == right);

        public override bool Equals(object? obj) => obj is Location location && Equals(location);

        public bool Equals(Location other)
            => CountryIndex == other.CountryIndex &&
                RegionIndex == other.RegionIndex &&
                CityIndex == other.CityIndex;

        public override int GetHashCode() => CityIndex.GetHashCode();
    }
}
