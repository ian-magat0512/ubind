// <copyright file="IProductPortalSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Repository for storing portal product settings.
    /// </summary>
    public interface IProductPortalSettingRepository
    {
        /// <summary>
        /// Creates a product portal setting for a product for multiple default organisation portals.
        /// </summary>
        void CreateSettingsForAllDefaultOrganisationPortals(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieve all portal setings from the system.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <returns>A collection of portal settings.</returns>
        IEnumerable<ProductPortalSettingModel> GetProductPortalSettings(Guid tenantId, Guid portalId);

        /// <summary>
        /// Retrieve all portal setings from the system.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>A collection of portal settings.</returns>
        IEnumerable<ProductPortalSettingModel> GetProductPortalSettings(Guid tenantId, string portalAlias);

        /// <summary>
        /// Adds or Updates portal product setting.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="isNewQuotesAllowed">Is new quotes allowed flag.</param>
        /// <returns>The updated product portal setting.</returns>
        ProductPortalSetting AddOrUpdateProductSetting(
            Guid tenantId, Guid portalId, Guid productId, bool isNewQuotesAllowed);
    }
}
