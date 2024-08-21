// <copyright file="DvaWebStartup.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Clients.DVA
{
    using System.Data.Entity;
    using System.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Clients.DVA.Perils.Services;
    using UBind.Domain.Clients.DVA.Perils.Interfaces;
    using UBind.Persistence.Clients.DVA.Migrations;
    using UBind.Persistence.Clients.DVA.Perils.Respositories;
    using UBind.Web.Configuration;

    /// <summary>
    /// DVA startup class.
    /// </summary>
    public static class DvaWebStartup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Collection to add services to.</param>
        /// <param name="configuration">The configuration settings instance.</param>
        public static void ConfigureServices(ref IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped<IPerilsService, PerilsService>();
            services.AddScoped<IPerilsRepository, PerilsRepository>();

            var connectionString = configuration.GetConnectionString("Dva");
            var automaticMigrationConfiguration = configuration
                .GetSection(nameof(AutomaticMigrationConfiguration))
                .Get<AutomaticMigrationConfiguration>();
            services.AddScoped(_ => new DvaDbContext(connectionString));

            var initializer = new MigrateDatabaseToLatestVersion<DvaDbContext, DvaConfiguration>(true);
            var allowDvaMigration = automaticMigrationConfiguration != null && automaticMigrationConfiguration.Dva;
            Database.SetInitializer(allowDvaMigration ? initializer : null);

            // Initialize DVA database only when we're not debugging or if it doesn't exist.
            // This saves around 3secs every run.
            if (!Debugger.IsAttached || !Database.Exists(connectionString))
            {
                using (var dbContext = new DvaDbContext(connectionString))
                {
                    dbContext.Database.Initialize(false);
                }
            }
        }
    }
}
