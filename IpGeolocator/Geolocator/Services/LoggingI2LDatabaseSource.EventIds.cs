using Microsoft.Extensions.Logging;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed partial class LoggingI2LDatabaseSource
    {
        private static class EventIds
        {
            public static readonly EventId Loading = new EventId(1, nameof(Loading));
            public static readonly EventId Loaded = new EventId(2, nameof(Loaded));
            public static readonly EventId LoadFailed = new EventId(3, nameof(LoadFailed));
        }
    }
}
