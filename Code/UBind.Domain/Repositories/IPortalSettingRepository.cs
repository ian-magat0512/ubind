// <copyright file="IPortalSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Repository for storing portal settings.
    /// </summary>
    public interface IPortalSettingRepository
    {
        /// <summary>
        /// Inserts a new portal setting into the repository.
        /// </summary>
        /// <param name="settings">New portal setting.</param>
        void Insert(PortalSettings settings);

        /// <summary>
        /// Adds a new portal setting detail to the portal setting.
        /// </summary>
        /// <param name="id">ID of setting to be updated.</param>
        /// <param name="details">The new record.</param>
        void Update(Guid id, PortalSettingDetails details);

        /// <summary>
        /// Retrieve all portal setings from the system.
        /// </summary>
        /// <returns>A collection of portal settings.</returns>
        IReadOnlyList<PortalSettings> GetAllPortalSettings();

        /// <summary>
        /// Retrieves a portal setting that matches the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The portal setting.</returns>
        PortalSettings GetPortalSettingsById(Guid id);

        /// <summary>
        /// Save any changes to portal settings.
        /// </summary>
        void SaveChanges();
    }
}
