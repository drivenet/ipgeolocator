using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

using IpGeolocator.Geolocator.Application;

using static System.FormattableString;

namespace IpGeolocator.Trash
{
    internal static partial class Test
    {
        public static void Run()
        {
            var datFile = @"C:\Users\xm\Downloads\IP-COUNTRY-REGION-CITY.DAT";
            //using (var input = File.OpenRead(@"C:\Users\xm\Downloads\IP-COUNTRY-REGION-CITY.CSV"))
            //{
            //    using (var output = File.Create(datFile, 4096, FileOptions.SequentialScan))
            //    {
            //        ConvertCsvToDat(input, output);
            //    }
            //}

            Database db;
            using (var input = File.OpenRead(datFile))
            {
                db = ReadDat(input);
            }

            var locator = new IP2LocationGeolocator(db);
            var ip = IPAddress.Parse("185.127.224.3");
            var sw = Stopwatch.StartNew();
            int i;
            for (i = 0; i < 50000000; i++)
            {
                locator.Geolocate(ip);
            }

            var location = locator.Geolocate(ip);
            Console.WriteLine("{0} {1} {2} {3}", location.Country, location.Region, location.City, i / sw.Elapsed.TotalSeconds);
        }

        private static void ConvertCsvToDat(Stream input, Stream output)
        {
            var nfi = NumberFormatInfo.InvariantInfo;
            var records = new List<LocationV4Record>();
            var atomMap = new Dictionary<string, int>() { [""] = 0 };
            var locationIndexMap = new Dictionary<Location, int>();
            using (var file = new StreamReader(input, Encoding.ASCII, false, 65536, true))
            {
                while (true)
                {
                    var line = file.ReadLine();
                    if (line is null)
                    {
                        break;
                    }

                    var span = line.AsSpan();
                    var offset = 1;
                    var fieldSpan = span.Slice(1);
                    fieldSpan = fieldSpan.Slice(0, fieldSpan.IndexOf('"'));
                    var fromAddress = uint.Parse(fieldSpan, NumberStyles.None, nfi);
                    offset += fieldSpan.Length + 3;
                    fieldSpan = span.Slice(offset);
                    fieldSpan = fieldSpan.Slice(0, fieldSpan.IndexOf('"'));
                    var toAddress = uint.Parse(fieldSpan, NumberStyles.None, nfi);
                    offset += fieldSpan.Length + 3;
                    var countrySpan = span.Slice(offset);
                    countrySpan = countrySpan.Slice(0, countrySpan.IndexOf('"'));
                    offset += countrySpan.Length + 3;
                    var country = countrySpan.ToString();
                    var countryIndex = Atomize(country);
                    if (countryIndex < 0)
                    {
                        continue;
                    }

                    fieldSpan = span.Slice(offset);
                    offset += fieldSpan.IndexOf('"') + 3;
                    var regionSpan = span.Slice(offset);
                    regionSpan = regionSpan.Slice(0, regionSpan.IndexOf('"'));
                    offset += regionSpan.Length + 3;
                    var regionIndex = Atomize(regionSpan.ToString());

                    var citySpan = span.Slice(offset);
                    citySpan = citySpan.Slice(0, citySpan.IndexOf('"'));
                    var cityIndex = Atomize(citySpan.ToString());

                    var locationKey = new Location(countryIndex, regionIndex, cityIndex);
                    if (!locationIndexMap.TryGetValue(locationKey, out var locationIndex))
                    {
                        locationIndex = locationIndexMap.Count;
                        locationIndexMap.Add(locationKey, locationIndex);
                    }

                    records.Add(new LocationV4Record(fromAddress, toAddress, locationIndex));
                }
            }

            var atoms = new string[atomMap.Count];
            foreach (var pair in atomMap)
            {
                atoms[pair.Value] = pair.Key;
            }

            var locations = new Location[locationIndexMap.Count];
            foreach (var pair in locationIndexMap)
            {
                locations[pair.Value] = pair.Key;
            }

            using var compressed = new DeflateStream(output, CompressionLevel.Optimal, true);
            using var buffered = new BufferedStream(compressed, 65536);
            using var writer = new BinaryWriter(buffered, Encoding.ASCII, true);
            writer.Write(1);
            writer.Write(atoms.Length);
            writer.Write(locations.Length);
            writer.Write(records.Count);
            foreach (var atom in atoms)
            {
                writer.Write(atom);
            }

            foreach (var location in locations)
            {
                writer.Write(location.CountryIndex);
                writer.Write(location.RegionIndex);
                writer.Write(location.CityIndex);
            }

            foreach (var item in records)
            {
                writer.Write(item.FromAddress);
                writer.Write(item.ToAddress);
                writer.Write(item.Index);
            }

            int Atomize(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value == "-")
                {
                    return 0;
                }

                if (!atomMap.TryGetValue(value, out var index))
                {
                    index = atomMap.Count;
                    atomMap.Add(value, index);
                }

                return index;
            }
        }

        private static Database ReadDat(Stream stream)
        {
            using var decompressed = new DeflateStream(stream, CompressionMode.Decompress, true);
            using var buffered = new BufferedStream(decompressed, 65536);
            using var reader = new BinaryReader(buffered, Encoding.ASCII, true);
            var version = reader.ReadInt32();
            if (version != 1)
            {
                throw new InvalidDataException(Invariant($"Invalid version {version}."));
            }

            var atomsCount = reader.ReadInt32();
            var locationsCount = reader.ReadInt32();
            var recordsCount = reader.ReadInt32();

            var atoms = new string[atomsCount];
            for (var i = 0; i < atomsCount; i++)
            {
                atoms[i] = reader.ReadString();
            }

            var locations = new Location[locationsCount];
            for (var i = 0; i < locationsCount; i++)
            {
                var countryIndex = reader.ReadInt32();
                var regionIndex = reader.ReadInt32();
                var cityIndex = reader.ReadInt32();
                locations[i] = new Location(countryIndex, regionIndex, cityIndex);
            }

            var records = new LocationV4Record[recordsCount];
            for (var i = 0; i < recordsCount; i++)
            {
                var fromAddress = reader.ReadUInt32();
                var toAddress = reader.ReadUInt32();
                var index = reader.ReadInt32();
                records[i] = new LocationV4Record(fromAddress, toAddress, index);
            }

            return new Database(records, locations, atoms);
        }
    }
}
