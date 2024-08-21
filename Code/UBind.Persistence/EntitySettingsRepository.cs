// <copyright file="EntitySettingsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for entity settings.
    /// </summary>
    public class EntitySettingsRepository : IEntitySettingsRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;

        public EntitySettingsRepository(
            IUBindDbContext dbContext,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.clock = clock;
        }

        public T GetEntitySettings<T>(Guid tenantId, EntityType entityType, Guid entityId)
            where T : IEntitySettings
        {
            var result = this.dbContext.EntitySettings
                .Where(s => s.TenantId == tenantId && s.EntityType == entityType && s.EntityId == entityId)
                .FirstOrDefault();

            return result != null ? result.GetSettings<T>() : (T)Activator.CreateInstance(typeof(T));
        }

        public EntityJsonSettings AddOrUpdateEntitySettings(
             Guid tenantId, EntityType entityType, Guid entityId, IEntitySettings configuration)
        {
            var setting = this.dbContext.EntitySettings
                .Where(s => s.TenantId == tenantId && s.EntityType == entityType && s.EntityId == entityId)
                .SingleOrDefault();
            if (setting == null)
            {
                var settingResult = EntityJsonSettings.Create(
                    tenantId, entityType, entityId, configuration, this.clock.Now());
                if (settingResult.IsFailure)
                {
                    throw new NotSupportedException();
                }

                setting = settingResult.Value;
            }

            setting.UpdateSettings(configuration);

            this.dbContext.EntitySettings.AddOrUpdate(setting);
            this.dbContext.SaveChanges();
            return setting;
        }
    }
}
