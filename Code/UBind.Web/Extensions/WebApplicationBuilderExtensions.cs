// <copyright file="WebApplicationBuilderExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Extensions
{
    using UBind.Application.Services;

    /// <summary>
    /// This class is used to create startup instance and configure start up services.
    /// </summary>
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplication Build<TStartup>(this WebApplicationBuilder builder)
        {
            var startup = Activator.CreateInstance(typeof(TStartup), new[] { builder.Environment });
            if (startup == null)
            {
                throw new InvalidOperationException("Could not instantiate Startup!");
            }

            var configureServices = typeof(TStartup).GetMethod("ConfigureServices");
            if (configureServices != null)
            {
                configureServices.Invoke(startup, new[] { builder.Services });
                var app = builder.Build();
                var configure = typeof(TStartup).GetMethod("Configure");
                if (configure == null)
                {
                    throw new InvalidOperationException("Could not find Configure on Startup!");
                }

                configure.Invoke(startup, new object[] { app, builder.Environment, builder.Services.BuildServiceProvider() });
                return app;
            }

            throw new InvalidOperationException("Could not find ConfigureServices on Startup!");
        }

        /// <summary>
        /// Makes sure a JWT key exists, and sets up a cron job to rotate keys daily.
        /// </summary>
        public static IApplicationBuilder UseJwtKeyRotation(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var jwtKeyRotator = serviceScope.ServiceProvider.GetRequiredService<IJwtKeyRotator>();

                // do initial rotation which will create a key for the first time if none exist
                jwtKeyRotator.RotateKeys();

                // trigger the daily cron job to rotate keys
                jwtKeyRotator.CreateKeyRotationJob();
            }

            return app;
        }
    }
}
