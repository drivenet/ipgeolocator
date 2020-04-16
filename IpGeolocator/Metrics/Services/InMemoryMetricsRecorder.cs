using System;
using System.Threading;

namespace IpGeolocator.Metrics.Services
{
    internal sealed class InMemoryMetricsRecorder : IMetricsRecorder, IMetricsSource, IMetricsManager
    {
        private long _successCount;
        private long _failureCount;
        private long _elapsedTicks;

        public ulong SuccessCount => unchecked((ulong)_successCount);

        public ulong FailureCount => unchecked((ulong)_failureCount);

        public TimeSpan Elapsed => TimeSpan.FromTicks(_elapsedTicks);

        public void RecordGeolocation(bool isSuccessful, TimeSpan elapsed)
        {
            if (isSuccessful)
            {
                Interlocked.Increment(ref _successCount);
            }
            else
            {
                Interlocked.Increment(ref _failureCount);
            }

            Interlocked.Add(ref _elapsedTicks, elapsed.Ticks);
        }

        public void Reset()
        {
            _successCount = 0;
            _failureCount = 0;
            _elapsedTicks = 0;
        }
    }
}
