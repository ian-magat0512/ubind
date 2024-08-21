// <copyright file="IPortalSettingsService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// Application service for handling portal settings related functionality.
    /// TODO: Refactor this into commands and queries.
    /// </summary>
    public interface IPortalSettingsService
    {
        /// <summary>
        /// Updates an existing portal with new portalsettings.
        /// </summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <param name="settingId">The parent setting.</param>
        /// <param name="value">The new values.</param>
        /// <returns>The updated portal.</returns>
        PortalReadModel UpdatePortalSettings(Guid tenantId, Guid portalId, Guid settingId, bool value);

        /// <summary>
        /// Gets the features enabeld for a given portal.
        /// </summary>
        /// <returns>A list of enabled features.</returns>
        /// <param name="portalId">The portal id.</param>
        IReadOnlyList<PortalSettings> GetPortalSettings(Guid tenantId, Guid portalId);
    }
}
