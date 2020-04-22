using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

using static System.FormattableString;

namespace IpGeolocator.Geolocator.Services
{
    internal sealed class StreamI2LDatabaseReader : II2LDatabaseReader
    {
        private readonly II2LDatabaseStreamFactory _streamFactory;

        public StreamI2LDatabaseReader(II2LDatabaseStreamFactory streamFactory)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
        }

        public I2LDatabase ReadDatabase()
        {
            using var stream = _streamFactory.Open();
            using var reader = new BinaryReader(new BufferedStream(stream), Encoding.ASCII);
            var version = reader.ReadInt32();
            DateTime timestamp;
            switch (version)
            {
                case 1:
                    timestamp = new DateTime(2020, 04, 21, 13, 18, 00, 0, DateTimeKind.Utc);
                    break;

                case 2:
                    timestamp = DateTime.FromBinary(reader.ReadInt64());
                    break;

                default:
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

            stream.Position = reader.BaseStream.Position;
            var buffer = Span<byte>.Empty;
            var locations = ReadLocations(stream, locationsCount, ref buffer);
            var intervals = ReadIntervals(stream, intervalsCount, ref buffer);

            if (stream.Position != stream.Length)
            {
                throw new InvalidDataException(Invariant($"Stream position {stream.Position} is not at end {stream.Length}."));
            }

            return new I2LDatabase(intervals, locations, atoms, timestamp);
        }

        private static I2LLocation[] ReadLocations(Stream stream, int locationsCount, ref Span<byte> buffer)
        {
            var locations = new I2LLocation[locationsCount];
            for (var chunkBase = 0; chunkBase < locationsCount;)
            {
                const int LocationsChunkSize = 5000;
                var chunkSize = Math.Min(locationsCount - chunkBase, LocationsChunkSize);
                var length = chunkSize * (sizeof(int) + sizeof(uint) + sizeof(int));
                if (buffer.Length < length)
                {
                    buffer = new byte[length];
                }

                var read = stream.Read(buffer.Slice(0, length));
                if (read != length)
                {
                    throw new InvalidDataException(Invariant($"Read {read} instead of {length} for locations."));
                }

                var offset = 0;
                for (var i = 0; i < chunkSize; i++)
                {
                    var countryIndex = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
                    offset += sizeof(int);
                    var regionIndex = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
                    offset += sizeof(int);
                    var cityIndex = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
                    offset += sizeof(int);
                    locations[chunkBase + i] = new I2LLocation(countryIndex, regionIndex, cityIndex);
                }

                chunkBase += chunkSize;
            }

            return locations;
        }

        private static I2LInterval4[] ReadIntervals(Stream stream, int intervalsCount, ref Span<byte> buffer)
        {
            var intervals = new I2LInterval4[intervalsCount];
            for (var chunkBase = 0; chunkBase < intervalsCount;)
            {
                const int IntervalsChunkSize = 5000;
                var chunkSize = Math.Min(intervalsCount - chunkBase, IntervalsChunkSize);
                var length = chunkSize * (sizeof(uint) + sizeof(uint) + sizeof(int));
                if (buffer.Length < length)
                {
                    buffer = new byte[length];
                }

                var read = stream.Read(buffer.Slice(0, length));
                if (read != length)
                {
                    throw new InvalidDataException(Invariant($"Read {read} instead of {length} for intervals."));
                }

                var offset = 0;
                for (var i = 0; i < chunkSize; i++)
                {
                    var fromAddress = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(offset));
                    offset += sizeof(uint);
                    var toAddress = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(offset));
                    offset += sizeof(uint);
                    var index = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
                    offset += sizeof(int);
                    intervals[chunkBase + i] = new I2LInterval4(fromAddress, toAddress, index);
                }

                chunkBase += chunkSize;
            }

            return intervals;
        }
    }
}
