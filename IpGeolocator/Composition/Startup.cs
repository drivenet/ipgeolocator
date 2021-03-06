﻿using System;

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
            ConfigureApplication(services);
            ConfigureAspNet(services);
        }

#pragma warning disable CA1822 // Mark members as static -- future-proofing
        public void Configure(IApplicationBuilder app)
#pragma warning restore CA1822 // Mark members as static
        {
            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapGet("/version", VersionHandler.Invoke);
                routes.MapPost("/v0.1/locate", app.ApplicationServices.GetRequiredService<LocateHandler>().Invoke);
                routes.MapGet("/v0.1/metrics/{" + MetricsHandler.TemplateName + "}", app.ApplicationServices.GetRequiredService<MetricsHandler>().Invoke);
                routes.MapGet("/v0.1/metadata/{" + MetadataHandler.TemplateName + "}", app.ApplicationServices.GetRequiredService<MetadataHandler>().Invoke);
            });
        }

        private static void ConfigureAspNet(IServiceCollection services)
        {
            services.AddRouting();
            services.AddSingleton<LocateHandler>();
            services.AddSingleton<MetricsHandler>();
            services.AddSingleton<MetadataHandler>();
        }

        private void ConfigureApplication(IServiceCollection services)
        {
            services.Configure<DatabaseOptions>(_configuration);
            services.AddSingleton<II2LDatabaseSource, CachingI2LDatabaseSource>();
            services.AddSingleton<II2LDatabaseStreamFactory, FileI2LDatabaseStreamFactory>();
            services.AddSingleton<StreamI2LDatabaseReader>();
            services.AddSingleton<II2LDatabaseReader>(provider =>
                new LoggingI2LDatabaseReader(
                    provider.GetRequiredService<StreamI2LDatabaseReader>(),
                    provider.GetRequiredService<ILogger<LoggingI2LDatabaseReader>>()));

            services.AddSingleton<IGeolocator>(provider =>
                new MetricsRecordingGeolocator(
                    new I2LGeolocator(provider.GetRequiredService<II2LDatabaseSource>()),
                    provider.GetRequiredService<IMetricsRecorder>()));

            services.AddSingleton<InMemoryMetricsRecorder>();
            static InMemoryMetricsRecorder MetricsRecorder(IServiceProvider provider) => provider.GetRequiredService<InMemoryMetricsRecorder>();
            services.AddSingleton<IMetricsSource>(MetricsRecorder);
            services.AddSingleton<IMetricsRecorder>(MetricsRecorder);
            services.AddSingleton<IMetricsManager>(MetricsRecorder);
        }
    }
}
