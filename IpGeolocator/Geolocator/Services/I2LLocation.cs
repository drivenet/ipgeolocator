using System;

namespace IpGeolocator.Geolocator.Services
{
    internal readonly struct I2LLocation : IEquatable<I2LLocation>
    {
        public I2LLocation(int countryIndex, int regionIndex, int cityIndex)
        {
            CountryIndex = countryIndex;
            RegionIndex = regionIndex;
            CityIndex = cityIndex;
        }

        public int CountryIndex { get; }

        public int RegionIndex { get; }

        public int CityIndex { get; }

        public static bool operator ==(I2LLocation left, I2LLocation right) => left.Equals(right);

        public static bool operator !=(I2LLocation left, I2LLocation right) => !(left == right);

        public override bool Equals(object? obj) => obj is I2LLocation location && Equals(location);

        public bool Equals(I2LLocation other)
            => CountryIndex == other.CountryIndex &&
                RegionIndex == other.RegionIndex &&
                CityIndex == other.CityIndex;

        public override int GetHashCode() => CityIndex.GetHashCode();
    }
}
