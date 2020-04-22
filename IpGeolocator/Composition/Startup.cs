using System;

using IpGeolocator.Geolocator;
using IpGeolocator.Geolocator.Services;
using IpGeolocator.Http;
using IpGeolocator.Metrics;
using IpGeolocator.Metrics.Services;

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
                routes.MapGet("/version", app.ApplicationServices.GetRequiredService<VersionHandler>().Invoke);
                routes.MapPost("/v0.1/locate", app.ApplicationServices.GetRequiredService<LocateHandler>().Invoke);
                routes.MapGet("/v0.1/metrics/{" + MetricsHandler.TemplateName + "}", app.ApplicationServices.GetRequiredService<MetricsHandler>().Invoke);
                routes.MapGet("/v0.1/metadata/{" + MetadataHandler.TemplateName + "}", app.ApplicationServices.GetRequiredService<MetadataHandler>().Invoke);
            });
        }

        private static void ConfigureAspNet(IServiceCollection services)
        {
            services.AddRouting();
            services.AddSingleton<LocateHandler>();
            services.AddSingleton<VersionHandler>();
            services.AddSingleton<MetricsHandler>();
            services.AddSingleton<MetadataHandler>();
        }

        private void ConfigureApplication(IServiceCollection services)
        {
            var databaseFileName = _configuration.GetValue("databaseFileName", "IP-COUNTRY-REGION-CITY.DAT");
            services.AddSingleton<II2LDatabaseSource, CachingI2LDatabaseSource>();
            services.AddSingleton<II2LDatabaseReader>(provider =>
                new LoggingI2LDatabaseReader(
                    new StreamI2LDatabaseReader(
                        new FileI2LDatabaseStreamFactory(databaseFileName)),
                    provider.GetService<ILogger<LoggingI2LDatabaseReader>>()));
            services.AddSingleton<IGeolocator>(provider =>
                new MetricsRecordingGeolocator(
                    new I2LGeolocator(provider.GetRequiredService<II2LDatabaseSource>()),
                    provider.GetRequiredService<IMetricsRecorder>()));
            services.AddSingleton<InMemoryMetricsRecorder>();
            services.AddSingleton<IMetricsSource>(provider => provider.GetRequiredService<InMemoryMetricsRecorder>());
            services.AddSingleton<IMetricsRecorder>(provider => provider.GetRequiredService<InMemoryMetricsRecorder>());
            services.AddSingleton<IMetricsManager>(provider => provider.GetRequiredService<InMemoryMetricsRecorder>());

            services.AddHostedService<PreheatingService>();
        }
    }
}
