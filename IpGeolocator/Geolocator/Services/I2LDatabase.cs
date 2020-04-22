﻿using System;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class I2LDatabase
    {
        public I2LDatabase(
            I2LInterval4[] intervals,
            I2LLocation[] locations,
            string[] atoms,
            DateTime timestamp)
        {
            Intervals = intervals ?? throw new ArgumentNullException(nameof(intervals));
            Locations = locations ?? throw new ArgumentNullException(nameof(locations));
            Atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
            Timestamp = timestamp.ToUniversalTime();
        }

        public I2LInterval4[] Intervals { get; }

        public I2LLocation[] Locations { get; }

        public string[] Atoms { get; }

        public DateTime Timestamp { get; }
    }
}
