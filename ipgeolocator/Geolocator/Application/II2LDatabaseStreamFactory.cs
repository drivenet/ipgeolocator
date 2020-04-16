using System.IO;

namespace IpGeolocator.Geolocator.Application
{
    internal interface II2LDatabaseStreamFactory
    {
        Stream Open();
    }
}
