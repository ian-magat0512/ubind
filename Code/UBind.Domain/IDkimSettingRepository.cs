// <copyright file="IDkimSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Entities;

    /// <summary>
    /// Repository for DKIM settings.
    /// </summary>
    public interface IDkimSettingRepository
    {
        /// <summary>
        /// Insert a new DKIM settings into the repository.
        /// </summary>
        /// <param name="dkimSettings">The deployment to insert.</param>
        DkimSettings Insert(DkimSettings dkimSettings);

        /// <summary>
        /// Update DKIM settings.
        /// </summary>
        DkimSettings Update(
            Guid tenantId,
            Guid dkimId,
            Guid organisationId,
            string domainName,
            string privateKey,
            string dnsSelector,
            string agentOrIdentifier,
            List<string> applicableDomainNameList);

        /// <summary>
        /// Get DKIM settings by tenant Id and Organisation.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        IEnumerable<DkimSettings> GetDkimSettingsByTenantIdAndOrganisationId(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Get DKIM settings by Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="dkimSettingsId">The DKIM settings Id.</param>
        DkimSettings? GetDkimSettingById(Guid tenantId, Guid organisationId, Guid dkimSettingsId);

        /// <summary>
        /// Get DKIM settings by organisation Id and domain name.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="domainName">The domain name.</param>
        IEnumerable<DkimSettings> GetDkimSettingsbyOrganisationIdAndDomainName(Guid tenantId, Guid organisationId, string domainName);

        /// <summary>
        /// Delete DKIM settings.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="dkimSettingsId">The DKIM settings Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        void Delete(Guid tenantId, Guid dkimSettingsId, Guid organisationId);

        /// <summary>
        /// Get DKIM settings by organisation Id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        IEnumerable<DkimSettings> GetDkimSettingsbyOrganisationId(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Save DKIM settings.
        /// </summary>
        Task SaveChangesAsync();
    }
}
