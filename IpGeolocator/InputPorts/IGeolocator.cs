using System.Net;

using IpGeolocator.Entities;

namespace IpGeolocator.InputPorts
{
    public interface IGeolocator
    {
        LocationInfo Geolocate(IPAddress address);
    }
}
