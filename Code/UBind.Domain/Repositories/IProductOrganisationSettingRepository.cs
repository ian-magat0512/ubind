// <copyright file="IProductOrganisationSettingRepository.cs" company="uBind">
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
    /// Repository for storing product organisation settings.
    /// </summary>
    public interface IProductOrganisationSettingRepository
    {
        /// <summary>
        /// Retrieve all organisation setings from the system.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <returns>A collection of organisation settings.</returns>
        IEnumerable<ProductOrganisationSettingModel> GetProductOrganisationSettings(
            Guid tenantId, Guid organisationId);

        ProductOrganisationSetting? GetProductOrganisationSetting(
            Guid tenantId, Guid organisationId, Guid productId);

        /// <summary>
        /// Update organisation product setting.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the current user to check for authorisation.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="isNewQuotesAllowed">Flag to determine if creation of new quotes is allowed in the organisation.</param>
        /// <returns>The updated product organisation setting.</returns>
        Task<ProductOrganisationSetting> UpdateProductSetting(
            Guid tenantId, Guid organisationId, Guid productId, bool isNewQuotesAllowed);
    }
}
