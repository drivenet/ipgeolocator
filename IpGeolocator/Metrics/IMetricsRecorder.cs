using System;

namespace IpGeolocator.Metrics
{
    public interface IMetricsRecorder
    {
        void RecordGeolocation(bool isSuccessful, TimeSpan elapsed);
    }
}
