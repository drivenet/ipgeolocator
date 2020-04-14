using System;

using IpGeolocator.Application;
using IpGeolocator.Http;
using IpGeolocator.InputPorts;
using IpGeolocator.Policy;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IpGeolocator.Composition
{
    internal sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddRouting();
            services.AddSingleton<LocateHandler>();

            services.AddSingleton<II2LDatabaseSource>(provider =>
                new CachingI2LDatabaseSource(
                    new StreamI2LDatabaseSource(
                        new FileI2LDatabaseStreamFactory(@"C:\Users\xm\Downloads\IP-COUNTRY-REGION-CITY.DAT")),
                    provider.GetService<ILogger<CachingI2LDatabaseSource>>()));
            services.AddSingleton<IGeolocator, I2LGeolocator>();
            services.AddHostedService<PreheatingService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapPost("/v0.1/locate", app.ApplicationServices.GetRequiredService<LocateHandler>().Invoke);
            });

#if false
            var locator = app.ApplicationServices.GetRequiredService<IGeolocator>();
            var ip = IPAddress.Parse("185.127.224.3");
            var location = locator.Geolocate(ip);
            int i;
            for (i = 0; i < 10000000; i++)
            {
                locator.Geolocate(ip);
            }

            var sw = Stopwatch.StartNew();
            for (i = 0; i < 100000000; i++)
            {
                locator.Geolocate(ip);
            }

            sw.Stop();

            Console.WriteLine("{0} {1} {2} {3}", location.Country, location.Region, location.City, i / sw.Elapsed.TotalSeconds);
#endif
        }
    }
}
