using System.Collections.Generic;
using System.Net;

using IpGeolocator.Geolocator.Entities;

namespace IpGeolocator.Geolocator.InputPorts
{
    public interface IGeolocator
    {
        IReadOnlyDictionary<IPAddress, LocationInfo> Geolocate(IReadOnlyCollection<IPAddress> addresses);
    }
}
