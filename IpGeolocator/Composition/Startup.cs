using System;

using IpGeolocator.Application;
using IpGeolocator.Http;
using IpGeolocator.InputPorts;
using IpGeolocator.Policy;

using Microsoft.AspNetCore.Builder;
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

            ConfigureApplication(services);
            ConfigureAspNet(services);
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
        }

        private static void ConfigureAspNet(IServiceCollection services)
        {
            services.AddRouting();
            services.AddSingleton<LocateHandler>();
        }

        private void ConfigureApplication(IServiceCollection services)
        {
            var databaseFileName = _configuration.GetValue("databaseFileName", "IP-COUNTRY-REGION-CITY.DAT");
            services.AddSingleton<II2LDatabaseSource>(provider =>
                new CachingI2LDatabaseSource(
                    new StreamI2LDatabaseSource(
                        new FileI2LDatabaseStreamFactory(databaseFileName)),
                    provider.GetService<ILogger<CachingI2LDatabaseSource>>()));
            services.AddSingleton<IGeolocator, I2LGeolocator>();

            services.AddHostedService<PreheatingService>();
        }
    }
}
