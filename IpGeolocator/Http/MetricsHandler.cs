using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using IpGeolocator.Metrics;

using Microsoft.AspNetCore.Http;

namespace IpGeolocator.Http
{
    internal sealed class MetricsHandler
    {
        public const string TemplateName = "metricName";

        private readonly IMetricsSource _metricsSource;

        public MetricsHandler(IMetricsSource metricsSource)
        {
            _metricsSource = metricsSource ?? throw new ArgumentNullException(nameof(metricsSource));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var metricName = (string)context.Request.RouteValues[TemplateName];
            var value = metricName switch
            {
                "location_successes" => _metricsSource.SuccessCount.ToString(NumberFormatInfo.InvariantInfo),
                "location_failures" => _metricsSource.FailureCount.ToString(NumberFormatInfo.InvariantInfo),
                "location_elapsed" => Math.Round(_metricsSource.Elapsed.TotalMilliseconds).ToString(NumberFormatInfo.InvariantInfo),
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
