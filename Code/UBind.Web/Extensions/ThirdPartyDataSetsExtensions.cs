// <copyright file="ThirdPartyDataSetsExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Extensions
{
    using System.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Persistence.Helpers;
    using UBind.Persistence.ThirdPartyDataSets;
    using UBind.Web.Configuration;

    public static class ThirdPartyDataSetsExtensions
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Collection to add services to.</param>
        /// <param name="configuration">The configuration settings instance.</param>
        public static void ConfigureThirdPartyDataSets(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var connectionString = configuration.GetConnectionString("ThirdPartyDataSets");
            var automaticMigrationConfiguration = configuration
                .GetSection(nameof(AutomaticMigrationConfiguration))
                .Get<AutomaticMigrationConfiguration>();

            var initializer
                = new MigrateDatabaseToLatestVersion<ThirdPartyDataSetsDbContext, ThirdPartyDataSetsDbContextConfiguration>(true);
            var allowThirdPartyDataSetsMigration = automaticMigrationConfiguration != null && automaticMigrationConfiguration.ThirdPartyDataSets;
            Database.SetInitializer(allowThirdPartyDataSetsMigration ? initializer : null);
            using (var dbContext = new ThirdPartyDataSetsDbContext(connectionString))
            {
#if DEBUG
                // Creating the database ourselves proves faster than relying on Database.Initialize() alone.
                DatabaseHelper.CreateDatabaseIfNotExists(connectionString);
#endif

                dbContext.Database.Initialize(false);
            }
        }
    }
}
