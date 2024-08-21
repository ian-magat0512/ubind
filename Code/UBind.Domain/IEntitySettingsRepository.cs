// <copyright file="IEntitySettingsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using UBind.Domain.Entities;
    using UBind.Domain.Models;

    /// <summary>
    /// Repository for entity settings.
    /// </summary>
    public interface IEntitySettingsRepository
    {
        EntityJsonSettings AddOrUpdateEntitySettings(
            Guid tenantId, EntityType entityType, Guid entityId, IEntitySettings configuration);

        T GetEntitySettings<T>(Guid tenantId, EntityType entityType, Guid entityId)
            where T : IEntitySettings;
    }
}
