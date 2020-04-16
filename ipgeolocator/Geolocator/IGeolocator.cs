using System.Net;

namespace IpGeolocator.Geolocator
{
    public interface IGeolocator
    {
        LocationInfo Geolocate(IPAddress address);
    }
}
