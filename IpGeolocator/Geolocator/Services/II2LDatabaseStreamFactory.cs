using System.IO;

namespace IpGeolocator.Geolocator.Services
{
    internal interface II2LDatabaseStreamFactory
    {
        Stream Open();
    }
}
