using System;
using System.Net;

using IpGeolocator.Geolocator.Entities;

namespace IpGeolocator.Geolocator.Application
{
    internal sealed class IP2LocationGeolocator
    {
        private readonly Database _db;

        public IP2LocationGeolocator(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public LocationInfo Geolocate(IPAddress address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return LocationInfo.Empty;
            }

#pragma warning disable CS0618 // Type or member is obsolete -- easiest way
            var addressValue = unchecked((uint)IPAddress.HostToNetworkOrder(unchecked((int)address.Address)));
#pragma warning restore CS0618 // Type or member is obsolete
            var min = 0;
            var records = _db.Records;
            var max = records.Length - 1;
            while (min <= max)
            {
                var midpoint = min + ((max - min) >> 1);
                var record = records[midpoint];
                var comparison = addressValue.CompareTo(record.FromAddress);
                if (comparison < 0)
                {
                    max = midpoint - 1;
                }
                else if (addressValue <= record.ToAddress)
                {
                    var location = _db.Locations[record.Index];
                    var atoms = _db.Atoms;
                    return new LocationInfo(
                        atoms[location.CountryIndex],
                        atoms[location.RegionIndex],
                        atoms[location.CityIndex]);
                }
                else
                {
                    min = midpoint + 1;
                }
            }

            return LocationInfo.Empty;
        }
    }
}
