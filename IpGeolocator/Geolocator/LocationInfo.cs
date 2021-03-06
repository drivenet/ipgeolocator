﻿using System;
using System.Collections.Generic;

namespace IpGeolocator.Geolocator
{
    public readonly struct LocationInfo : IEquatable<LocationInfo>
    {
        private readonly string? _country;
        private readonly string? _region;
        private readonly string? _city;

        public LocationInfo(string country, string region, string city)
        {
            _country = country;
            _region = region;
            _city = city;
        }

        public readonly string Country => _country ?? "";

        public readonly string Region => _region ?? "";

        public readonly string City => _city ?? "";

        public static bool operator ==(LocationInfo left, LocationInfo right) => EqualityComparer<LocationInfo>.Default.Equals(left, right);

        public static bool operator !=(LocationInfo left, LocationInfo right) => !(left == right);

        public override readonly bool Equals(object? obj) => obj is LocationInfo info && Equals(info);

        public readonly bool Equals(LocationInfo other)
            => Country == other.Country &&
                Region == other.Region &&
                City == other.City;

        public override readonly int GetHashCode() => HashCode.Combine(Country, Region, City);
    }
}
