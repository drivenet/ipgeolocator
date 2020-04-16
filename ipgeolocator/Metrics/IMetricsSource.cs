using System;

namespace IpGeolocator.Metrics
{
    public interface IMetricsSource
    {
        ulong SuccessCount { get; }

        ulong FailureCount { get; }

        TimeSpan Elapsed { get; }
    }
}
