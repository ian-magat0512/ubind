// <copyright file="DkimSettingsUpsertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// The DKIM Settings.
    /// </summary>
    public class DkimSettingsUpsertModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DkimSettingsUpsertModel"/> class.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="id">The DKIM settings Id.</param>
        /// <param name="organisationId">The organisationId.</param>
        /// <param name="domainName">The domain name.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="dnsSelector">The DNS selector.</param>
        /// <param name="agentOrIdentifier">The agent or identifier.</param>
        /// <param name="applicableDomainNameList">The applicable domain name list.</param>
        public DkimSettingsUpsertModel(
            string tenant,
            Guid id,
            Guid organisationId,
            string domainName,
            string privateKey,
            string dnsSelector,
            string agentOrIdentifier,
            List<string> applicableDomainNameList)
        {
            this.Tenant = tenant;
            this.Id = id;
            this.OrganisationId = organisationId;
            this.DomainName = domainName;
            this.PrivateKey = privateKey;
            this.DnsSelector = dnsSelector;
            this.AgentOrUserIdentifier = agentOrIdentifier;
            this.ApplicableDomainNameList = applicableDomainNameList;
        }

        /// <summary>
        /// Gets the DKIM settings Id.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the tenant ID or alias.
        /// </summary>
        [JsonProperty]
        public string Tenant { get; private set; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        [JsonProperty]
        public string DomainName { get; private set; }

        /// <summary>
        /// Gets the agent or user identifier.
        /// </summary>
        [JsonProperty]
        public string AgentOrUserIdentifier { get; private set; }

        /// <summary>
        /// Gets the private key.
        /// </summary>
        [JsonProperty]
        public string PrivateKey { get; private set; }

        /// <summary>
        /// Gets the DNS selector.
        /// </summary>
        [JsonProperty]
        public string DnsSelector { get; private set; }

        /// <summary>
        /// Gets the Applicable domain name list.
        /// </summary>
        [JsonProperty]
        public List<string> ApplicableDomainNameList { get; private set; }

        /// <summary>
        /// Updates DKIM settings.
        /// </summary>
        /// <param name="domainName">The domain name.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="dnsSelector">The DNS selector.</param>
        /// <param name="applicableDomainNameList">The applicable domain name list.</param>
        public void Update(
            string domainName,
            string privateKey,
            string dnsSelector,
            List<string> applicableDomainNameList)
        {
            this.DomainName = domainName;
            this.PrivateKey = privateKey;
            this.DnsSelector = dnsSelector;
            this.DomainName = domainName;
            this.ApplicableDomainNameList = applicableDomainNameList;
        }
    }
}
