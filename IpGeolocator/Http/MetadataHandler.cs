using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

using IpGeolocator.Geolocator.Services;

using Microsoft.AspNetCore.Http;

namespace IpGeolocator.Http
{
    internal sealed class MetadataHandler
    {
        public const string TemplateName = "metadataName";

        private readonly II2LDatabaseSource _dbSource;

        public MetadataHandler(II2LDatabaseSource dbSource)
        {
            _dbSource = dbSource ?? throw new ArgumentNullException(nameof(dbSource));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var metadataName = (string)context.Request.RouteValues[TemplateName];
            var value = metadataName switch
            {
                "db_timestamp" => _dbSource.Database.Timestamp.ToString("o", NumberFormatInfo.InvariantInfo),
                "db_intervals" => _dbSource.Database.Intervals.Length.ToString(NumberFormatInfo.InvariantInfo),
                _ => null,
            };

            if (value is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            await context.Response.WriteAsync(value);
        }
    }
}
