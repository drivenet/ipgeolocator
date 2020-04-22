using Microsoft.Extensions.Logging;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed partial class LoggingI2LDatabaseReader
    {
        private static class EventIds
        {
            public static readonly EventId Reading = new EventId(1, nameof(Reading));
            public static readonly EventId Read = new EventId(2, nameof(Read));
            public static readonly EventId ReadFailed = new EventId(3, nameof(ReadFailed));
        }
    }
}
