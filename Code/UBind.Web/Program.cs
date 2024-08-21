// <copyright file="Program.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web
{
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Sentry;
    using Sentry.AspNetCore;
    using Serilog;
    using UBind.Application.Configuration;
    using UBind.Application.Exceptions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// The web application.
    /// </summary>
    public class Program
    {
        private static IHost host = null!;

        /// <summary>
        /// Entry point for web application.
        /// </summary>
        public static void Main(string[] args)
        {
            host = CreateHostBuilder(args).UseSerilog().Build();
            host.Run();
        }

        /// <summary>
        /// Create host builder.
        /// </summary>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder? webHostBuilder = null;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_LAUNCHPROFILE") == "BrowserstackLocalHttps")
            {
                webHostBuilder = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHost(builder =>
                    {
                        builder.UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Loopback, 443, listenOptions =>
                            {
                                listenOptions.UseHttps("ssl/bs-local.com.pfx", "password");
                            });
                        });
                        builder.UseUrls("https://bs-local.com");
                        builder.UseContentRoot(Directory.GetCurrentDirectory());
                        builder.UseSentry(SentryOptionsConfiguration);
                        builder.UseStartup<Startup>();
                    });
            }
            else
            {
                webHostBuilder = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHost(builder =>
                    {
                        builder.UseKestrel(options => options.AddServerHeader = false);
                        builder.UseContentRoot(Directory.GetCurrentDirectory());
                        builder.UseSentry(SentryOptionsConfiguration);
                        builder.UseStartup<Startup>();
                    });
            }

            return webHostBuilder;
        }

        /// <summary>
        /// Sentry options configuration.
        /// </summary>
        private static void SentryOptionsConfiguration(SentryAspNetCoreOptions options)
        {
            var sentryExtrasConfig = host.Services.GetRequiredService<IConfiguration>().
                GetSection("SentryExtrasConfiguration").
                Get<SentryExtrasConfiguration>();
            sentryExtrasConfig = EntityHelper.ThrowIfNotFound(sentryExtrasConfig, "ExcludedExceptions");

            options.KeepAggregateException = true;
            options.AddExceptionProcessor(new SentryEventExceptionProcessor() { SentryExtrasConfig = sentryExtrasConfig });
            options.AddEventProcessor(new SentryEventExclusionFilterProcessor() { SentryExtrasConfig = sentryExtrasConfig });
        }
    }
}
