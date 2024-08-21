// <copyright file="DkimSettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// The DKIM Settings.
    /// </summary>
    public class DkimSettings : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DkimSettings"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="domainName">The domain name.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="dnsSelector">The DNS selector.</param>
        /// <param name="agentOrIdentifier">The Agent or Identifier.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public DkimSettings(
            Guid tenantId,
            Guid organisationId,
            string domainName,
            string privateKey,
            string dnsSelector,
            string agentOrIdentifier,
            List<string> applicableDomainNameList,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.DomainName = domainName;
            this.PrivateKey = privateKey;
            this.DnsSelector = dnsSelector;
            this.AgentOrUserIdentifier = agentOrIdentifier;
            this.ApplicableDomainNameListJSON = applicableDomainNameList == null ? null : JsonConvert.SerializeObject(applicableDomainNameList);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DkimSettings"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private DkimSettings()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string DomainName { get; private set; }

        /// <summary>
        /// Gets the private key.
        /// </summary>
        public string PrivateKey { get; private set; }

        /// <summary>
        /// Gets the dns selector.
        /// </summary>
        public string DnsSelector { get; private set; }

        /// <summary>
        /// Gets the agent or user identifier.
        /// </summary>
        public string AgentOrUserIdentifier { get; private set; }

        /// <summary>
        /// Gets the Applicable Domain JSON.
        /// </summary>
        public string ApplicableDomainNameListJSON { get; private set; }

        /// <summary>
        /// Gets the list of Applicable Domain name.
        /// </summary>
        public List<string> ApplicableDomainNameList
        {
            get
            {
                if (string.IsNullOrEmpty(this.ApplicableDomainNameListJSON))
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<List<string>>(this.ApplicableDomainNameListJSON);
            }
        }

        /// <summary>
        /// Updates DKIM settings.
        /// </summary>
        public void Update(
            string domainName,
            string privateKey,
            string dnsSelector,
            string agentOrIdentifier,
            List<string> applicableDomainNameList)
        {
            this.DomainName = domainName;
            this.PrivateKey = privateKey;
            this.DnsSelector = dnsSelector;
            this.AgentOrUserIdentifier = agentOrIdentifier;
            this.DomainName = domainName;
            this.ApplicableDomainNameListJSON = applicableDomainNameList == null ? null : JsonConvert.SerializeObject(applicableDomainNameList);
        }
    }
}
