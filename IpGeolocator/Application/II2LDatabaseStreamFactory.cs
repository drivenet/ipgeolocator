using System.IO;

namespace IpGeolocator.Application
{
    internal interface II2LDatabaseStreamFactory
    {
        Stream Open();
    }
}
