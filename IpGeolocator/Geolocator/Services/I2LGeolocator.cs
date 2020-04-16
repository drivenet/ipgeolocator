using System;
using System.Net;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class I2LGeolocator : IGeolocator
    {
        private readonly II2LDatabaseSource _dbSource;

        public I2LGeolocator(II2LDatabaseSource dbSource)
        {
            _dbSource = dbSource ?? throw new ArgumentNullException(nameof(dbSource));
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

#pragma warning disable CS0618 // Type or member is obsolete -- this is the fastest way to access IPv4 address
            var addressAsNumber = address.Address;
#pragma warning restore CS0618 // Type or member is obsolete
            var addressValue = unchecked((uint)IPAddress.HostToNetworkOrder(unchecked((int)addressAsNumber)));
            var db = _dbSource.Database;
            var min = 0;
            var intervals = db.Intervals;
            var max = intervals.Length - 1;
            while (min <= max)
            {
                var midpoint = min + ((max - min) / 2);
                var interval = intervals[midpoint];
                if (addressValue < interval.FromAddress)
                {
                    max = midpoint - 1;
                }
                else if (addressValue > interval.ToAddress)
                {
                    min = midpoint + 1;
                }
                else
                {
                    var location = db.Locations[interval.Index];
                    var atoms = db.Atoms;
                    return new LocationInfo(
                        atoms[location.CountryIndex],
                        atoms[location.RegionIndex],
                        atoms[location.CityIndex]);
                }
            }

            return LocationInfo.Empty;
        }
    }
}
