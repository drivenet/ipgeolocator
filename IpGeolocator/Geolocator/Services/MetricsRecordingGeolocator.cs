﻿using System;
using System.Diagnostics;
using System.Net;

using IpGeolocator.Metrics;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class MetricsRecordingGeolocator : IGeolocator
    {
        private static readonly double StopwatchTickScale = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

        private readonly IGeolocator _inner;
        private readonly IMetricsRecorder _metricsRecorder;

        public MetricsRecordingGeolocator(IGeolocator inner, IMetricsRecorder metricsRecorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _metricsRecorder = metricsRecorder ?? throw new ArgumentNullException(nameof(metricsRecorder));
        }

        public LocationInfo Geolocate(IPAddress address)
        {
            var startTime = Stopwatch.GetTimestamp();
            var location = _inner.Geolocate(address);
            var elapsedTicks = Stopwatch.GetTimestamp() - startTime;
            var isSuccessful = location != default;
            var elapsed = TimeSpan.FromTicks(checked((long)(elapsedTicks * StopwatchTickScale)));
            _metricsRecorder.RecordGeolocation(isSuccessful, elapsed);
            return location;
        }
    }
}
