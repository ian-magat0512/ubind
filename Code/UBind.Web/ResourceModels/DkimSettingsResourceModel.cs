// <copyright file="DkimSettingsResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Entities;
    using UBind.Domain.Helpers;

    /// <summary>
    /// This class is used to display DKIM Settings.
    /// </summary>
    public class DkimSettingsResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DkimSettingsResourceModel"/> class.
        /// </summary>
        /// <param name="dkimSettings">The DKIM settings.</param>
        public DkimSettingsResourceModel(DkimSettings dkimSettings)
        {
            this.TenantId = dkimSettings.TenantId;
            this.Id = dkimSettings.Id;
            this.OrganisationId = dkimSettings.OrganisationId;
            this.DomainName = dkimSettings.DomainName;
            this.PrivateKey = CryptographyHelper.MaskPrivateKey(dkimSettings.PrivateKey);
            this.DnsSelector = dkimSettings.DnsSelector;
            this.AgentOrUserIdentifier = dkimSettings.AgentOrUserIdentifier;
            this.ApplicableDomainNameList = dkimSettings.ApplicableDomainNameList;
        }

        /// <summary>
        /// Gets the DKIM settings Id.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; private set; }

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
        /// Gets the product key.
        /// </summary>
        [JsonProperty]
        public string PrivateKey { get; private set; }

        /// <summary>
        /// Gets the Agent or Identifier.
        /// </summary>
        [JsonProperty]
        public string AgentOrUserIdentifier { get; private set; }

        /// <summary>
        /// Gets the dns selector.
        /// </summary>
        [JsonProperty]
        public string DnsSelector { get; private set; }

        /// <summary>
        /// Gets the Applicable domain name list.
        /// </summary>
        [JsonProperty]
        public List<string> ApplicableDomainNameList { get; private set; }
    }
}
