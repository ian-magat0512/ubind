// <copyright file="DkimSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class DkimSettingRepository : IDkimSettingRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DkimSettingRepository(IUBindDbContext dbContext, IClock clock)
        {
            this.dbContext = dbContext;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public DkimSettings Update(
            Guid tenantId,
            Guid dkimSettingsId,
            Guid organisationId,
            string domainName,
            string privateKey,
            string dnsSelector,
            string agentOrIdentifier,
            List<string> applicableDomainNameList)
        {
            var dkimSetting = this.dbContext.DkimSettings.FirstOrDefault(s => s.Id == dkimSettingsId && s.TenantId == tenantId && s.OrganisationId == organisationId);
            dkimSetting.Update(domainName, privateKey, dnsSelector, agentOrIdentifier, applicableDomainNameList);
            return dkimSetting;
        }

        /// <inheritdoc/>
        public void Delete(Guid tenantId, Guid dkimSettingsId, Guid organisationId)
        {
            var dkimSetting = this.dbContext.DkimSettings.FirstOrDefault(s => s.Id == dkimSettingsId && s.TenantId == tenantId && s.OrganisationId == organisationId);
            this.dbContext.DkimSettings.Remove(dkimSetting);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public DkimSettings Insert(DkimSettings dkimSettings)
        {
            var dkimSetting = new DkimSettings(dkimSettings.TenantId, dkimSettings.OrganisationId, dkimSettings.DomainName, dkimSettings.PrivateKey, dkimSettings.DnsSelector, dkimSettings.AgentOrUserIdentifier, dkimSettings.ApplicableDomainNameList, this.clock.Now());
            return this.dbContext.DkimSettings.Add(dkimSetting);
        }

        /// <inheritdoc/>
        public IEnumerable<DkimSettings> GetDkimSettingsbyOrganisationIdAndDomainName(Guid tenantId, Guid organisationId, string domainName)
        {
            var dkimSettings = this.dbContext.DkimSettings.Where(s => s.OrganisationId == organisationId && s.TenantId == tenantId && (s.DomainName == domainName || "mail." + s.DomainName == domainName)).ToList();
            return dkimSettings;
        }

        /// <inheritdoc/>
        public IEnumerable<DkimSettings> GetDkimSettingsbyOrganisationId(Guid tenantId, Guid organisationId)
        {
            var dkimSettings = this.dbContext.DkimSettings.Where(s => s.OrganisationId == organisationId && s.TenantId == tenantId).ToList();
            return dkimSettings;
        }

        /// <inheritdoc/>
        public IEnumerable<DkimSettings> GetDkimSettingsByTenantIdAndOrganisationId(Guid tenantId, Guid organisationId)
        {
            return this.dbContext.DkimSettings.Where(s => s.TenantId == tenantId && s.OrganisationId == organisationId).OrderBy(o => o.DomainName);
        }

        /// <inheritdoc/>
        public DkimSettings? GetDkimSettingById(Guid tenantId, Guid organisationId, Guid dkimSettingsId)
        {
            return this.dbContext.DkimSettings.Where(s => s.Id == dkimSettingsId && s.TenantId == tenantId && s.OrganisationId == organisationId).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            await this.dbContext.SaveChangesAsync();
        }
    }
}
