﻿using System;
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
            var commandLineOptions = GetCommandLineOptions(args);
            var appConfiguration = LoadAppConfiguration(commandLineOptions.Config);
            var hostingConfigPath = commandLineOptions.HostingConfig;
            do
            {
                await RunHost(appConfiguration, hostingConfigPath);
            }
            while (ServiceManager.IsRunningAsService);
        }

        private static void Convert()
        {
            using var input = Console.OpenStandardInput();
            using var output = Console.OpenStandardOutput();
            Geolocator.Helpers.DatabaseUtils.ConvertFromCsv(input, output, DateTime.UtcNow);
        }

        private static async Task RunHost(IConfiguration appConfiguration, string hostingConfigPath)
        {
            var hostingOptions = GetHostingOptions(hostingConfigPath);
            using var host = BuildHost(hostingOptions, appConfiguration);
            await host.RunAsync();
        }

        private static IConfiguration LoadAppConfiguration(string configPath)
            => new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

        private static CommandLineOptions GetCommandLineOptions(string[] args)
            => new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()
                .Get<CommandLineOptions>() ?? new CommandLineOptions();

        private static HostingOptions GetHostingOptions(string configPath)
            => new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false)
                .Build()
                .Get<HostingOptions>() ?? new HostingOptions();

        private static IHost BuildHost(HostingOptions hostingOptions, IConfiguration appConfiguration)
            => new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseUrls(hostingOptions.Listen)
                    .UseKestrel(options => ConfigureKestrel(options, hostingOptions))
                    .UseLibuv()
                    .UseStartup<Startup>())
                .ConfigureLogging(loggingBuilder => ConfigureLogging(loggingBuilder, hostingOptions))
                .UseSystemd()
                .ConfigureAppConfiguration(configBuilder => configBuilder.AddConfiguration(appConfiguration))
                .Build();

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder, HostingOptions hostingOptions)
        {
            loggingBuilder.AddFilter(
                (category, level) => level >= LogLevel.Warning
                    || (level >= LogLevel.Information && !category.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase)));
            var hasJournalD = Journal.IsSupported;
            if (hasJournalD)
            {
                loggingBuilder.AddJournal(options =>
                {
                    options.SyslogIdentifier = "ipgeolocator";
                });
            }

            if (!hasJournalD || hostingOptions.ForceConsoleLogging)
            {
                loggingBuilder.AddConsole(options => options.DisableColors = true);
            }
        }

        private static void ConfigureKestrel(KestrelServerOptions options, HostingOptions hostingOptions)
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = 1 << 20;
            options.Limits.MaxRequestHeadersTotalSize = 8192;

            var maxConcurrentConnections = hostingOptions.MaxConcurrentConnections;
            if (maxConcurrentConnections != 0)
            {
                options.Limits.MaxConcurrentConnections = maxConcurrentConnections;
            }

            options.UseSystemd();
        }
    }
}
