using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Tmds.Systemd;

namespace IpGeolocator.Composition
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var argsCount = args.Length;
            if (argsCount > 0)
            {
                switch (args[0])
                {
                    case "convert":
                        if (argsCount == 1)
                        {
                            Convert();
                            return 0;
                        }

                        break;

                    case "serve":
                        {
                            var serveArgs = new string[argsCount - 1];
                            Array.Copy(args, 1, serveArgs, 0, serveArgs.Length);
                            await Serve(serveArgs);
                            return 0;
                        }
                }
            }

            Console.Error.WriteLine(
                "ipgeolocator serve [--config appsettings.json] [--hostingconfig hostingsettings.json]" + Environment.NewLine
                + "ipgeolocator convert < IP-COUNTRY-REGION-CITY.CSV > IP-COUNTRY-REGION-CITY.DAT");
            return -1;
        }

        private static async Task Serve(string[] args)
        {
            using var host = BuildHost(args);
            await host.RunAsync();
        }

        private static void Convert()
        {
            using var input = Console.OpenStandardInput();
            using var output = Console.OpenStandardOutput();
            Geolocator.Helpers.DatabaseUtils.ConvertFromCsv(input, output, DateTime.UtcNow);
        }

        private static IHost BuildHost(string[] args)
            => new HostBuilder()
                .ConfigureHostConfiguration(configBuilder => configBuilder.AddCommandLine(args))
                .ConfigureWebHost(webHost => ConfigureWebHost(webHost))
                .ConfigureLogging((builderContext, loggingBuilder) => ConfigureLogging(builderContext, loggingBuilder))
#if !MINIMAL_BUILD
                .UseSystemd()
#endif
                .ConfigureAppConfiguration((builderContext, configBuilder) => ConfigureAppConfiguration(args, builderContext, configBuilder))
                .Build();

        private static IWebHostBuilder ConfigureWebHost(IWebHostBuilder webHost)
            => webHost
                .UseKestrel((builderContext, options) => ConfigureKestrel(builderContext, options))
                .UseStartup<Startup>();

#if MINIMAL_BUILD
#pragma warning disable CA1801 // Review unused parameters -- required for other build configuration
#endif
        private static void ConfigureLogging(HostBuilderContext builderContext, ILoggingBuilder loggingBuilder)
#if MINIMAL_BUILD
#pragma warning restore CA1801 // Review unused parameters
#endif
        {
            loggingBuilder.AddFilter(
                (category, level) => level >= LogLevel.Warning
                    || (level >= LogLevel.Information && !category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase)));

#if !MINIMAL_BUILD
            if (Journal.IsSupported)
            {
                loggingBuilder.AddJournal(options =>
                {
                    options.SyslogIdentifier = builderContext.HostingEnvironment.ApplicationName;
                });
            }
#endif

#if !MINIMAL_BUILD
            if (builderContext.Configuration.GetValue<bool>("ForceConsoleLogging")
                 || !Journal.IsAvailable)
#endif
            {
                loggingBuilder.AddSystemdConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffffzzz \""
                        + Environment.MachineName
                        + "\" \""
                        + builderContext.HostingEnvironment.ApplicationName
                        + ":\" ";
                });
            }
        }

        private static void ConfigureKestrel(WebHostBuilderContext builderContext, KestrelServerOptions options)
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestLineSize = 1024;
            options.Limits.MaxRequestBodySize = 1 << 20;
            options.Limits.MaxRequestHeadersTotalSize = 8192;

            var kestrelSection = builderContext.Configuration.GetSection("Kestrel");
            options.Configure(kestrelSection);

            if (kestrelSection.Get<KestrelServerOptions>() is { } kestrelOptions)
            {
                options.Limits.MaxConcurrentConnections = kestrelOptions.Limits.MaxConcurrentConnections;
            }
            else
            {
                options.Limits.MaxConcurrentConnections = 10;
            }

#if !MINIMAL_BUILD
#if NET6_0_OR_GREATER
            options.UseSystemd();
#else
            // SD_LISTEN_FDS_START https://www.freedesktop.org/software/systemd/man/sd_listen_fds.html
            const int SdListenFdsStart = 3;
            const string ListenFdsEnvVar = "LISTEN_FDS";

            options.UseSystemd(listenOptions =>
            {
                if (listenOptions.FileHandle == SdListenFdsStart)
                {
                    // This matches sd_listen_fds behavior that requires %LISTEN_FDS% to be present and in range [1;INT_MAX-SD_LISTEN_FDS_START]
                    if (int.TryParse(Environment.GetEnvironmentVariable(ListenFdsEnvVar), System.Globalization.NumberStyles.None, System.Globalization.NumberFormatInfo.InvariantInfo, out var listenFds)
                        && listenFds > 1
                        && listenFds <= int.MaxValue - SdListenFdsStart)
                    {
                        for (var handle = SdListenFdsStart + 1; handle < SdListenFdsStart + listenFds; ++handle)
                        {
                            options.ListenHandle((ulong)handle);
                        }
                    }
                }
            });
#endif
#endif
        }

        private static IConfigurationBuilder ConfigureAppConfiguration(string[] args, HostBuilderContext builderContext, IConfigurationBuilder configBuilder)
            => configBuilder
                .AddJsonFile(builderContext.Configuration.GetValue("ConfigPath", "appsettings.json"), optional: false, reloadOnChange: true)
                .AddCommandLine(args);
    }
}
