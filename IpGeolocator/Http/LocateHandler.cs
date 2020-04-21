using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using IpGeolocator.Geolocator;

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
            var requestEncoding = Encoding.ASCII;
            if (MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType)
                && (contentType.MediaType != "text/plain" ||
                    (contentType.CharSet is string charSet && charSet != requestEncoding.WebName)))
            {
                response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                return;
            }

            using var input = new StreamReader(request.Body, encoding: requestEncoding, detectEncodingFromByteOrderMarks: false, bufferSize: 256, leaveOpen: true);
            response.ContentType = "text/plain";
            await Process(input, response.Body);
        }

        private static int CreateResponseLine(Span<byte> buffer, string requestLine, LocationInfo locationInfo)
        {
            var responseEncoding = Encoding.ASCII;
            var offset = 0;
            offset += responseEncoding.GetBytes(requestLine, buffer.Slice(offset));
            buffer[offset++] = unchecked((byte)'\t');
            offset += responseEncoding.GetBytes(locationInfo.Country, buffer.Slice(offset));
            buffer[offset++] = unchecked((byte)'\t');
            offset += responseEncoding.GetBytes(locationInfo.Region, buffer.Slice(offset));
            buffer[offset++] = unchecked((byte)'\t');
            offset += responseEncoding.GetBytes(locationInfo.City, buffer.Slice(offset));
            buffer[offset++] = unchecked((byte)'\n');
            return offset;
        }

        private async Task Process(TextReader input, Stream output)
        {
            var requestLineTask = input.ReadLineAsync();
            var responseLineTask = default(ValueTask);
            var buffer = new byte[256];
            while (true)
            {
                string? requestLine;
                try
                {
                    requestLine = await requestLineTask;
                }
                catch (IOException)
                {
                    break;
                }

                if (requestLine is null)
                {
                    break;
                }

                requestLineTask = input.ReadLineAsync();
                if (!IPAddress.TryParse(requestLine, out var address))
                {
                    continue;
                }

                var locationInfo = _geolocator.Geolocate(address);
                if (locationInfo == default)
                {
                    continue;
                }

                await responseLineTask;
                var length = CreateResponseLine(buffer, requestLine, locationInfo);
                responseLineTask = output.WriteAsync(buffer.AsMemory(0, length));
            }
        }
    }
}
