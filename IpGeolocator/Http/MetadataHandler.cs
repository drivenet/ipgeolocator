using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using IpGeolocator.Geolocator.Services;

using Microsoft.AspNetCore.Http;

namespace IpGeolocator.Http
{
    internal sealed class MetadataHandler
    {
        public const string TemplateName = "metadataName";

        private static readonly DateTime UtcEpoch = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);

        private readonly II2LDatabaseSource _dbSource;

        public MetadataHandler(II2LDatabaseSource dbSource)
        {
            _dbSource = dbSource ?? throw new ArgumentNullException(nameof(dbSource));
        }

        public async Task Invoke(HttpContext context)
        {
            var metadataName = (string)context.Request.RouteValues[TemplateName];
            var value = metadataName switch
            {
                "db_timestamp" => _dbSource.Database.Timestamp.ToString("o", NumberFormatInfo.InvariantInfo),
                "db_timestamp_unix" => ((_dbSource.Database.Timestamp - UtcEpoch).Ticks / TimeSpan.TicksPerSecond).ToString(NumberFormatInfo.InvariantInfo),
                "db_intervals" => _dbSource.Database.Intervals.Length.ToString(NumberFormatInfo.InvariantInfo),
                _ => null,
            };

            var response = context.Response;
            if (value is null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var result = Encoding.ASCII.GetBytes(value);
            response.ContentLength = result.Length;
            await response.Body.WriteAsync(result);
        }
    }
}
