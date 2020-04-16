using System;
using System.IO;
using System.Text;

using static System.FormattableString;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class StreamI2LDatabaseSource : II2LDatabaseSource
    {
        private readonly II2LDatabaseStreamFactory _streamFactory;

        public StreamI2LDatabaseSource(II2LDatabaseStreamFactory streamFactory)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        }

        public I2LDatabase Database
        {
            get
            {
                using var stream = _streamFactory.Open();
                using var reader = new BinaryReader(new BufferedStream(stream, 65536), Encoding.ASCII);
                var version = reader.ReadInt32();
                if (version != 1)
                {
                    throw new InvalidDataException(Invariant($"Invalid version {version}."));
                }

                var atomsCount = reader.ReadInt32();
                var locationsCount = reader.ReadInt32();
                var intervalsCount = reader.ReadInt32();

                var atoms = new string[atomsCount];
                for (var i = 0; i < atomsCount; i++)
                {
                    atoms[i] = reader.ReadString();
                }

                var locations = new I2LLocation[locationsCount];
                for (var i = 0; i < locationsCount; i++)
                {
                    var countryIndex = reader.ReadInt32();
                    var regionIndex = reader.ReadInt32();
                    var cityIndex = reader.ReadInt32();
                    locations[i] = new I2LLocation(countryIndex, regionIndex, cityIndex);
                }

                var intervals = new I2LInterval4[intervalsCount];
                for (var i = 0; i < intervalsCount; i++)
                {
                    var fromAddress = reader.ReadUInt32();
                    var toAddress = reader.ReadUInt32();
                    var index = reader.ReadInt32();
                    intervals[i] = new I2LInterval4(fromAddress, toAddress, index);
                }

                return new I2LDatabase(intervals, locations, atoms);
            }
        }
    }
}
