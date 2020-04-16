using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using IpGeolocator.Geolocator.Services;

using static System.FormattableString;

namespace IpGeolocator.Geolocator.Helpers
{
    internal static class DatabaseUtils
    {
        public static void ConvertFromCsv(Stream input, Stream output)
        {
            var nfi = NumberFormatInfo.InvariantInfo;
            var intervals = new List<I2LInterval4>();
            var atomMap = new Dictionary<string, int>() { [""] = 0 };
            var locationIndexMap = new Dictionary<I2LLocation, int>();
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

                    var locationKey = new I2LLocation(countryIndex, regionIndex, cityIndex);
                    if (!locationIndexMap.TryGetValue(locationKey, out var locationIndex))
                    {
                        locationIndex = locationIndexMap.Count;
                        locationIndexMap.Add(locationKey, locationIndex);
                    }

                    intervals.Add(new I2LInterval4(fromAddress, toAddress, locationIndex));
                }
            }

            const int MinIntervals = 8000000;
            if (intervals.Count < MinIntervals)
            {
                throw new InvalidDataException(Invariant($"The database records count {intervals.Count} is less than minimum allowed {MinIntervals}."));
            }

            var atoms = new string[atomMap.Count];
            foreach (var pair in atomMap)
            {
                atoms[pair.Value] = pair.Key;
            }

            var locations = new I2LLocation[locationIndexMap.Count];
            foreach (var pair in locationIndexMap)
            {
                locations[pair.Value] = pair.Key;
            }

            using var writer = new BinaryWriter(new BufferedStream(output, 65536), Encoding.ASCII, true);
            writer.Write(1);
            writer.Write(atoms.Length);
            writer.Write(locations.Length);
            writer.Write(intervals.Count);
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

            foreach (var item in intervals)
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
    }
}
