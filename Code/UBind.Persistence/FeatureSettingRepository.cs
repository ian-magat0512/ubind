// <copyright file="FeatureSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using MoreLinq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Temporary in-memory repository for use during UI development.
    /// </summary>
    public class FeatureSettingRepository : IFeatureSettingRepository
    {
        private readonly IClock clock;
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSettingRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="clock">The clock.</param>
        public FeatureSettingRepository(
            IUBindDbContext dbContext,
            IClock clock)
        {
            this.clock = clock;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public Setting GetSettingById(string settingId)
        {
            return this.dbContext.Settings.Single(s => s.Id == settingId);
        }

        /// <inheritdoc/>
        public IEnumerable<Setting> GetSettings(EntityListFilters filters)
        {
            var query = this.dbContext.Settings.IncludeAllProperties();

            if (filters.SearchTerms.Any())
            {
                var searchExpression = LinqKit.PredicateBuilder.New<Setting>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(s =>
                        s.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        s.Details.Tenant.Id.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                query = query.Where(searchExpression);
            }

            return query.ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Setting> GetSettings(Guid tenantId)
        {
            using (MiniProfiler.Current.Step($"{nameof(FeatureSettingRepository)}.{nameof(this.GetSettings)}"))
            {
                var settingsQuery = this.dbContext.Settings.IncludeAllProperties();

                var settings = settingsQuery.ToList();

                settings = settings.Select(s => new Setting(s.Id, s.Name, s.Icon, s.SortOrder, s.CreatedTimestamp, s.IconLibrary)
                {
                    DetailsCollection = new Collection<SettingDetails>(s?.DetailsCollection.Where(x => x.Tenant?.Id == tenantId).ToList())
                        ?? new Collection<SettingDetails> { new SettingDetails(true, null, (Instant)s.CreatedTimestamp) },
                }).ToList();

                return settings.OrderBy(s => s.SortOrder).ToList();
            }
        }

        /// <inheritdoc/>
        public void SetInitialSettings(Guid tenantId)
        {
            var settingsQuery = this.dbContext.Settings.IncludeAllProperties();

            var tenant = this.dbContext.Tenants.FirstOrDefault(x => x.Id == tenantId);
            foreach (var setting in settingsQuery)
            {
                setting.Update(new SettingDetails(false, tenant, this.clock.Now()));
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Setting> GetActiveSettings(Guid tenantId)
        {
            using (MiniProfiler.Current.Step($"{nameof(FeatureSettingRepository)}.{nameof(this.GetActiveSettings)}"))
            {
                return this.GetSettings(tenantId)
                .Where(setting => !setting.Details?.Disabled ?? false);
            }
        }

        /// <inheritdoc/>
        public void Insert(Setting setting)
        {
            this.dbContext.Settings.Add(setting);
        }

        /// <inheritdoc/>
        public void Upsert(Setting setting)
        {
            this.dbContext.Settings.AddOrUpdate(setting);
        }

        /// <inheritdoc/>
        public void PopulateForAllTenants(IEnumerable<Setting> settings)
        {
            settings.ForEach((setting) => { this.dbContext.Settings.AddOrUpdate(x => x.Id, setting); });
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }
    }
}
