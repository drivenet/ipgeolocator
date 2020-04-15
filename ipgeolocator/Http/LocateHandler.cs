using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using IpGeolocator.InputPorts;

using Microsoft.AspNetCore.Http;

namespace IpGeolocator.Http
{
    internal sealed class LocateHandler
    {
        private readonly IGeolocator _geolocator;

        public LocateHandler(IGeolocator geolocator)
        {
            _geolocator = geolocator ?? throw new ArgumentNullException(nameof(geolocator));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var request = httpContext.Request;
            var response = httpContext.Response;
            var exchangeEncoding = Encoding.ASCII;
            if (MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType)
                && (contentType.MediaType != "text/plain" ||
                    (contentType.CharSet is string charSet && charSet != exchangeEncoding.WebName)))
            {
                response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                return;
            }

            using var reader = new StreamReader(request.Body, encoding: exchangeEncoding, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var requestLineTask = reader.ReadLineAsync();
            var responseLineTask = Task.CompletedTask;
            response.ContentType = "text/plain";
            while (true)
            {
                var requestLine = await requestLineTask;
                if (requestLine is null)
                {
                    break;
                }

                requestLineTask = reader.ReadLineAsync();
                requestLine = requestLine.Trim();
                if (!IPAddress.TryParse(requestLine, out var address))
                {
                    continue;
                }

                var locationInfo = _geolocator.Geolocate(address);
                var responseLine = string.Concat(requestLine, "\t", locationInfo.Country, "\t", locationInfo.Region, "\t", locationInfo.City, "\n");
                await responseLineTask;
                responseLineTask = response.WriteAsync(responseLine, exchangeEncoding);
            }

            await responseLineTask;
        }
    }
}
